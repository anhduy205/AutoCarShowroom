namespace AutoCarShowroom.Models
{
    public static class OrderWorkflow
    {
        public const string CarStatusAvailable = "Còn hàng";
        public const string CarStatusSold = "Đã bán";
        public const string CarStatusPromotion = "Khuyến mãi";

        public const string OrderStatusNew = "Mới tạo";
        public const string OrderStatusPaid = "Đã thanh toán";
        public const string OrderStatusProcessing = "Đang xử lý";
        public const string OrderStatusCompleted = "Hoàn tất";
        public const string OrderStatusCancelled = "Đã hủy";

        public const string PaymentStatusUnpaid = "Chưa thanh toán";
        public const string PaymentStatusPaid = "Đã thanh toán";

        public const string PaymentMethodSimulation = "Thanh toán mô phỏng";

        public static readonly string[] PurchasableCarStatuses =
        [
            CarStatusAvailable,
            CarStatusPromotion
        ];

        public static readonly string[] OrderStatuses =
        [
            OrderStatusNew,
            OrderStatusPaid,
            OrderStatusProcessing,
            OrderStatusCompleted,
            OrderStatusCancelled
        ];

        public static readonly string[] PaymentStatuses =
        [
            PaymentStatusUnpaid,
            PaymentStatusPaid
        ];

        public static readonly string[] PaymentMethods =
        [
            PaymentMethodSimulation
        ];

        public static readonly string[] LockingOrderStatuses =
        [
            OrderStatusPaid,
            OrderStatusProcessing,
            OrderStatusCompleted
        ];

        public static bool CanOrder(Car car)
        {
            return PurchasableCarStatuses.Contains(car.Status, StringComparer.OrdinalIgnoreCase);
        }
    }
}
