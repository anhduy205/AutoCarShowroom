using System.Text;
using AutoCarShowroom.Models;

namespace AutoCarShowroom.Services.Chatbot
{
    public static class ChatbotTextEncodingHelper
    {
        private static readonly Encoding StrictWindows1252;
        private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        static ChatbotTextEncodingHelper()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var windows1252 = (Encoding)Encoding.GetEncoding(1252).Clone();
            windows1252.EncoderFallback = EncoderFallback.ExceptionFallback;
            windows1252.DecoderFallback = DecoderFallback.ExceptionFallback;
            StrictWindows1252 = windows1252;
        }

        public static string NormalizeText(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value ?? string.Empty;
            }

            var current = value.Normalize(NormalizationForm.FormC);

            for (var attempt = 0; attempt < 3; attempt++)
            {
                if (GetSuspiciousScore(current) == 0)
                {
                    break;
                }

                var decoded = TryDecodeUtf8Mojibake(current);
                if (string.IsNullOrWhiteSpace(decoded))
                {
                    break;
                }

                decoded = decoded.Normalize(NormalizationForm.FormC);
                if (GetSuspiciousScore(decoded) >= GetSuspiciousScore(current))
                {
                    break;
                }

                current = decoded;
            }

            return current;
        }

        public static ChatbotReply NormalizeReply(ChatbotReply reply)
        {
            reply.Message = NormalizeText(reply.Message);
            reply.Suggestions = (reply.Suggestions ?? [])
                .Select(NormalizeSuggestion)
                .ToList();
            reply.QuickReplies = (reply.QuickReplies ?? [])
                .Select(NormalizeQuickReply)
                .ToList();
            reply.Actions = (reply.Actions ?? [])
                .Select(NormalizeAction)
                .ToList();

            return reply;
        }

        public static ChatbotFaqArticle NormalizeArticle(ChatbotFaqArticle article)
        {
            article.Topic = NormalizeText(article.Topic);
            article.Title = NormalizeText(article.Title);
            article.Keywords = (article.Keywords ?? [])
                .Select(NormalizeText)
                .ToList();
            article.Answer = NormalizeText(article.Answer);
            article.NextStep = NormalizeText(article.NextStep);
            article.Disclaimer = NormalizeText(article.Disclaimer);

            return article;
        }

        private static ChatbotSuggestion NormalizeSuggestion(ChatbotSuggestion suggestion)
        {
            suggestion.CarName = NormalizeText(suggestion.CarName);
            suggestion.Brand = NormalizeText(suggestion.Brand);
            suggestion.ModelName = NormalizeText(suggestion.ModelName);
            suggestion.BodyType = NormalizeText(suggestion.BodyType);
            suggestion.Status = NormalizeText(suggestion.Status);
            suggestion.Reason = NormalizeText(suggestion.Reason);

            return suggestion;
        }

        private static ChatbotQuickReply NormalizeQuickReply(ChatbotQuickReply reply)
        {
            reply.Label = NormalizeText(reply.Label);
            reply.Message = NormalizeText(reply.Message);

            return reply;
        }

        private static ChatbotAction NormalizeAction(ChatbotAction action)
        {
            action.Label = NormalizeText(action.Label);
            action.Message = NormalizeText(action.Message);

            return action;
        }

        private static string? TryDecodeUtf8Mojibake(string value)
        {
            try
            {
                return StrictUtf8.GetString(StrictWindows1252.GetBytes(value));
            }
            catch
            {
                return null;
            }
        }

        private static int GetSuspiciousScore(string value)
        {
            var score = 0;
            score += CountOccurrences(value, "Ã");
            score += CountOccurrences(value, "Â");
            score += CountOccurrences(value, "Ä");
            score += CountOccurrences(value, "Æ");
            score += CountOccurrences(value, "�");
            score += CountOccurrences(value, "á»") * 2;
            score += CountOccurrences(value, "áº") * 2;
            score += CountOccurrences(value, "â€") * 2;
            score += CountOccurrences(value, "â€¢") * 2;

            return score;
        }

        private static int CountOccurrences(string value, string fragment)
        {
            var count = 0;
            var index = 0;

            while ((index = value.IndexOf(fragment, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += fragment.Length;
            }

            return count;
        }
    }
}
