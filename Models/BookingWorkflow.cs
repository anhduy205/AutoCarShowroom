namespace AutoCarShowroom.Models
{
    public static class BookingWorkflow
    {
        public const string StatusNew = "Mới đặt";
        public const string StatusConfirmed = "Đã xác nhận";
        public const string StatusVisited = "Đã đến hẹn";
        public const string StatusSold = "Đã bán";
        public const string StatusUnsuccessful = "Không bán được";
        public const string StatusCancelled = "Đã hủy";

        public static readonly string[] Statuses =
        [
            StatusNew,
            StatusConfirmed,
            StatusVisited,
            StatusSold,
            StatusUnsuccessful,
            StatusCancelled
        ];
    }
}
