namespace AutoCarShowroom.Models
{
    public static class OrderWorkflow
    {
        public const string CarStatusAvailable = "Còn hàng";
        public const string CarStatusSold = "Đã bán";
        public const string CarStatusPromotion = "Khuyến mãi";

        public const string OrderStatusNew = "Mới tạo";
        public const string OrderStatusAwaitingConfirmation = "Chờ xác nhận";
        public const string OrderStatusPaid = "Đã thanh toán";
        public const string OrderStatusProcessing = "Đang xử lý";
        public const string OrderStatusCompleted = "Hoàn tất";
        public const string OrderStatusCancelled = "Đã hủy";

        public const string PaymentStatusUnpaid = "Chưa thanh toán";
        public const string PaymentStatusPending = "Chờ xác nhận";
        public const string PaymentStatusPaid = "Đã thanh toán";

        public const string PaymentMethodQr = "Thanh toán QR showroom";
        public const string PaymentMethodBankTransfer = "Chuyển khoản ngân hàng";
        public const string PaymentMethodCash = "Thanh toán tại showroom";
        public const string PaymentMethodInstallment = "Hỗ trợ trả góp / đặt cọc";
        public const string PaymentMethodSimulation = PaymentMethodQr;

        public static readonly string[] PurchasableCarStatuses =
        [
            CarStatusAvailable,
            CarStatusPromotion
        ];

        public static readonly string[] OrderStatuses =
        [
            OrderStatusNew,
            OrderStatusAwaitingConfirmation,
            OrderStatusPaid,
            OrderStatusProcessing,
            OrderStatusCompleted,
            OrderStatusCancelled
        ];

        public static readonly string[] PaymentStatuses =
        [
            PaymentStatusUnpaid,
            PaymentStatusPending,
            PaymentStatusPaid
        ];

        public static readonly PaymentMethodInfo[] PaymentMethodInfos =
        [
            new(
                PaymentMethodQr,
                "Thanh toán QR",
                "Khách chọn quét QR showroom. Hệ thống tạo yêu cầu mua trước, nhân viên xác nhận giao dịch sau.",
                "Xác nhận nhanh",
                [
                    "Quét mã QR hoặc chuyển khoản theo thông tin showroom ở bước sau.",
                    "Dùng mã đơn làm nội dung chuyển khoản để đội ngũ đối soát.",
                    "Nhân viên sẽ gọi lại sau khi giao dịch được xác nhận."
                ],
                PaymentStatusPending,
                RequiresManualConfirmation: true),
            new(
                PaymentMethodBankTransfer,
                "Chuyển khoản",
                "Khách chuyển khoản ngân hàng sau khi gửi yêu cầu mua. Đơn ở trạng thái chờ xác nhận thanh toán.",
                "Đối soát thủ công",
                [
                    "Hệ thống tạo mã đơn riêng cho từng giao dịch.",
                    "Đội ngũ nội bộ xác nhận giao dịch ngân hàng trước khi xử lý bàn giao.",
                    "Có thể ghi chú nhu cầu xuất hóa đơn hoặc đặt cọc trong ô ghi chú."
                ],
                PaymentStatusPending,
                RequiresManualConfirmation: true),
            new(
                PaymentMethodCash,
                "Thanh toán tại showroom",
                "Khách để lại thông tin mua xe và thanh toán trực tiếp khi đến showroom hoặc lúc bàn giao.",
                "Linh hoạt",
                [
                    "Đơn được ghi nhận ngay để nhân viên giữ xe và hẹn lịch tư vấn.",
                    "Thanh toán sẽ được cập nhật thủ công khi khách đến showroom.",
                    "Phù hợp khi khách muốn xem xe trước khi xuống tiền."
                ],
                PaymentStatusUnpaid,
                RequiresManualConfirmation: false),
            new(
                PaymentMethodInstallment,
                "Trả góp / đặt cọc",
                "Khách để lại nhu cầu trả góp hoặc đặt cọc. Nhân viên sẽ liên hệ để tư vấn hồ sơ và lộ trình thanh toán.",
                "Cần tư vấn",
                [
                    "Đơn mua được giữ ở trạng thái chờ xác nhận.",
                    "Nhân viên sẽ liên hệ để chốt khoản cọc hoặc hồ sơ hỗ trợ vay.",
                    "Khách nên mô tả thêm nhu cầu trong ô ghi chú để xử lý nhanh hơn."
                ],
                PaymentStatusPending,
                RequiresManualConfirmation: true)
        ];

        public static readonly string[] PaymentMethods = PaymentMethodInfos
            .Select(item => item.Value)
            .ToArray();

        public static readonly string[] LockingOrderStatuses =
        [
            OrderStatusNew,
            OrderStatusAwaitingConfirmation,
            OrderStatusPaid,
            OrderStatusProcessing,
            OrderStatusCompleted
        ];

        public static bool CanOrder(Car car)
        {
            return PurchasableCarStatuses.Contains(car.Status, StringComparer.OrdinalIgnoreCase);
        }

        public static PaymentMethodInfo GetPaymentMethodInfo(string? paymentMethod)
        {
            return PaymentMethodInfos.FirstOrDefault(item =>
                       string.Equals(item.Value, paymentMethod, StringComparison.OrdinalIgnoreCase))
                   ?? PaymentMethodInfos[0];
        }

        public static string GetInitialOrderStatus(string? paymentMethod)
        {
            return GetPaymentMethodInfo(paymentMethod).RequiresManualConfirmation
                ? OrderStatusAwaitingConfirmation
                : OrderStatusNew;
        }

        public static string GetInitialPaymentStatus(string? paymentMethod)
        {
            return GetPaymentMethodInfo(paymentMethod).InitialPaymentStatus;
        }

        public static bool UsesTransferInstructions(string? paymentMethod)
        {
            return string.Equals(paymentMethod, PaymentMethodQr, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(paymentMethod, PaymentMethodBankTransfer, StringComparison.OrdinalIgnoreCase);
        }

        public sealed record PaymentMethodInfo(
            string Value,
            string ShortLabel,
            string Description,
            string Highlight,
            IReadOnlyList<string> Steps,
            string InitialPaymentStatus,
            bool RequiresManualConfirmation);
    }
}
