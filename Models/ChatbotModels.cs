namespace AutoCarShowroom.Models
{
    public class ChatbotRequest
    {
        public string Message { get; set; } = string.Empty;

        public IReadOnlyList<ChatbotHistoryMessage> History { get; set; } = [];

        public int? CurrentCarId { get; set; }

        public ChatbotConversationState? State { get; set; }
    }

    public class ChatbotReply
    {
        public string Message { get; set; } = string.Empty;

        public IReadOnlyList<ChatbotSuggestion> Suggestions { get; set; } = [];

        public IReadOnlyList<ChatbotQuickReply> QuickReplies { get; set; } = [];

        public IReadOnlyList<ChatbotAction> Actions { get; set; } = [];

        public ChatbotConversationState? State { get; set; }

        public bool RequiresInput { get; set; }
    }

    public class ChatbotHistoryMessage
    {
        public string Role { get; set; } = "assistant";

        public string Message { get; set; } = string.Empty;
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

        public string Status { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public bool CanOrder { get; set; }
    }

    public class ChatbotQuickReply
    {
        public string Label { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }

    public class ChatbotAction
    {
        public string Label { get; set; } = string.Empty;

        public string Kind { get; set; } = "link";

        public string Variant { get; set; } = "secondary";

        public string? Url { get; set; }

        public string? Message { get; set; }
    }

    public class ChatbotConversationState
    {
        public string? ActiveMode { get; set; }

        public string? ActiveIntent { get; set; }

        public string? PendingField { get; set; }

        public int? CurrentCarId { get; set; }

        public int ClarifyingQuestionsAsked { get; set; }

        public ChatbotSearchProfile SearchProfile { get; set; } = new();

        public ChatbotBookingDraft BookingDraft { get; set; } = new();

        public ChatbotInstallmentDraft InstallmentDraft { get; set; } = new();
    }

    public class ChatbotSearchProfile
    {
        public decimal? Budget { get; set; }

        public string? Purpose { get; set; }

        public string? BodyType { get; set; }

        public int? SeatCount { get; set; }

        public string? Brand { get; set; }

        public string? RawKeywords { get; set; }
    }

    public class ChatbotBookingDraft
    {
        public int? CarId { get; set; }

        public string? CustomerName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public DateTime? AppointmentAt { get; set; }

        public string? Note { get; set; }
    }

    public class ChatbotInstallmentDraft
    {
        public int? CarId { get; set; }

        public decimal? DownPayment { get; set; }

        public int? TermMonths { get; set; }

        public decimal? AnnualInterestRate { get; set; }
    }
}
