using AutoCarShowroom.Models;

namespace AutoCarShowroom.ViewModels
{
    public class RevenueDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }

        public int TotalTransactions { get; set; }

        public int OrderRevenueCount { get; set; }

        public int BookingRevenueCount { get; set; }

        public IReadOnlyList<RevenueRecord> Records { get; set; } = [];

        public RevenueRecordFormViewModel Form { get; set; } = new();
    }
}
