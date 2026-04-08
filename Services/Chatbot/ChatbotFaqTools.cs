using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace AutoCarShowroom.Services.Chatbot
{
    public sealed class ChatbotFaqTools
    {
        private readonly IReadOnlyList<ChatbotFaqArticle> _articles;

        public ChatbotFaqTools(IHostEnvironment environment)
        {
            _articles = LoadArticles(environment.ContentRootPath);
        }

        public ChatbotFaqArticle? Lookup(string? message)
        {
            var normalizedMessage = ChatbotTextParser.Normalize(message);
            if (string.IsNullOrWhiteSpace(normalizedMessage))
            {
                return null;
            }

            return _articles
                .Select(article => new
                {
                    Article = article,
                    Score = article.Keywords.Count(keyword =>
                        normalizedMessage.Contains(ChatbotTextParser.Normalize(keyword), StringComparison.Ordinal))
                })
                .Where(item => item.Score > 0)
                .OrderByDescending(item => item.Score)
                .ThenByDescending(item => item.Article.Keywords.Count)
                .Select(item => item.Article)
                .FirstOrDefault();
        }

        private static IReadOnlyList<ChatbotFaqArticle> LoadArticles(string contentRootPath)
        {
            var filePath = Path.Combine(contentRootPath, "Data", "chatbot-faq.json");
            if (!File.Exists(filePath))
            {
                return [];
            }

            try
            {
                var json = File.ReadAllText(filePath);
                return (JsonSerializer.Deserialize<List<ChatbotFaqArticle>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? [])
                    .Select(ChatbotTextEncodingHelper.NormalizeArticle)
                    .ToList();
            }
            catch
            {
                return [];
            }
        }
    }
}
