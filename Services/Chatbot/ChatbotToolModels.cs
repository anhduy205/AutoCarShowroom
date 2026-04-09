using AutoCarShowroom.Models;

namespace AutoCarShowroom.Services.Chatbot
{
    public sealed class ChatbotSearchCriteria
    {
        public decimal? MinBudget { get; set; }

        public decimal? MaxBudget { get; set; }

        public string? Brand { get; set; }

        public string? BodyType { get; set; }

        public int? SeatCount { get; set; }

        public string? Purpose { get; set; }

        public string? Keyword { get; set; }

        public int? ExcludedCarId { get; set; }

        public int MaxResults { get; set; } = 5;
    }

    public sealed class ChatbotBudgetConstraint
    {
        public decimal Amount { get; init; }

        public decimal? MinBudget { get; init; }

        public decimal? MaxBudget { get; init; }

        public string Mode { get; init; } = "under";
    }

    public sealed class ChatbotCarMatch
    {
        public required Car Car { get; init; }

        public int Score { get; init; }

        public IReadOnlyList<string> MatchedReasons { get; init; } = [];

        public IReadOnlyList<string> Considerations { get; init; } = [];
    }

    public sealed class ChatbotInventoryOverview
    {
        public int TotalVisibleCars { get; init; }

        public decimal MinPrice { get; init; }

        public decimal MaxPrice { get; init; }

        public IReadOnlyList<string> BodyTypeSummaries { get; init; } = [];

        public IReadOnlyList<string> BrandSummaries { get; init; } = [];

        public IReadOnlyList<Car> FeaturedCars { get; init; } = [];
    }

    public sealed class ChatbotInstallmentEstimate
    {
        public required Car Car { get; init; }

        public decimal CarPrice { get; init; }

        public decimal DownPayment { get; init; }

        public decimal LoanAmount { get; init; }

        public int TermMonths { get; init; }

        public decimal AnnualInterestRate { get; init; }

        public decimal MonthlyPayment { get; init; }

        public decimal RegistrationEstimate { get; init; }

        public decimal InsuranceEstimate { get; init; }

        public decimal MonthlyFuelEstimate { get; init; }

        public decimal MonthlyMaintenanceEstimate { get; init; }
    }

    public sealed class ChatbotBookingCreationResult
    {
        public bool Succeeded { get; init; }

        public Booking? Booking { get; init; }

        public Car? Car { get; init; }

        public string CustomerMessage { get; init; } = string.Empty;

        public DateTime? SuggestedAppointmentAt { get; init; }

        public IReadOnlyList<string> Errors { get; init; } = [];
    }

    public sealed class ChatbotFaqArticle
    {
        public string Topic { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public IReadOnlyList<string> Keywords { get; set; } = [];

        public string Answer { get; set; } = string.Empty;

        public string NextStep { get; set; } = string.Empty;

        public string Disclaimer { get; set; } = string.Empty;
    }
}
