namespace AutoCarShowroom.Models
{
    public static class RevenueWorkflow
    {
        public const string SourceTypeOrder = "Đơn mua";
        public const string SourceTypeBooking = "Booking";
        public const string SourceTypeOther = "Khác";

        public static readonly string[] SourceTypes =
        [
            SourceTypeOrder,
            SourceTypeBooking,
            SourceTypeOther
        ];
    }
}
