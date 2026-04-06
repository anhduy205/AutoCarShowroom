namespace AutoCarShowroom.Models
{
    public class InstallmentOptions
    {
        public decimal DefaultAnnualInterestRate { get; set; } = 8.5m;

        public int DefaultTermMonths { get; set; } = 60;

        public decimal DefaultDownPaymentRatio { get; set; } = 0.3m;

        public decimal RegistrationFeeRate { get; set; } = 0.1m;

        public decimal InsuranceRate { get; set; } = 0.015m;

        public decimal MonthlyFuelEstimate { get; set; } = 3_500_000m;

        public decimal MonthlyMaintenanceEstimate { get; set; } = 1_000_000m;
    }
}
