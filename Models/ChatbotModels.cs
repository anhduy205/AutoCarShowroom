namespace AutoCarShowroom.Models
{
    public class ChatbotRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ChatbotReply
    {
        public string Message { get; set; } = string.Empty;

        public IReadOnlyList<ChatbotSuggestion> Suggestions { get; set; } = [];
    }

    public class ChatbotSuggestion
    {
        public int CarId { get; set; }

        public string CarName { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string ModelName { get; set; } = string.Empty;

        public string BodyType { get; set; } = string.Empty;

        public string Image { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
