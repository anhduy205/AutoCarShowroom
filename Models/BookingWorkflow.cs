namespace AutoCarShowroom.Models
{
    public static class BookingWorkflow
    {
        public const string StatusPendingConfirmation = "Chờ xác nhận";
        public const string StatusConfirmed = "Đã xác nhận";
        public const string StatusRejected = "Đã từ chối";
        public const string StatusRescheduleRequested = "Đề nghị đổi giờ";
        public const string StatusVisited = "Đã đến hẹn";
        public const string StatusSold = "Đã bán";
        public const string StatusUnsuccessful = "Không bán được";
        public const string StatusCancelled = "Đã hủy";

        public const string ServiceViewing = "Xem xe";
        public const string ServiceConsultation = "Tư vấn";
        public const string ServiceTestDrive = "Lái thử";

        public const int MaxRequestsPerSlot = 1;
        public const int BusySlotThreshold = 1;

        public static readonly string[] Statuses =
        [
            StatusPendingConfirmation,
            StatusConfirmed,
            StatusRejected,
            StatusRescheduleRequested,
            StatusVisited,
            StatusSold,
            StatusUnsuccessful,
            StatusCancelled
        ];

        public static readonly string[] ServiceTypes =
        [
            ServiceViewing,
            ServiceConsultation,
            ServiceTestDrive
        ];

        public static readonly string[] SlotCountingStatuses =
        [
            StatusPendingConfirmation,
            StatusConfirmed
        ];

        public static DateTime NormalizeSlot(DateTime appointmentAt)
        {
            return new DateTime(
                appointmentAt.Year,
                appointmentAt.Month,
                appointmentAt.Day,
                appointmentAt.Hour,
                0,
                0,
                appointmentAt.Kind);
        }

        public static string BuildCustomerConfirmationMessage(Booking booking)
        {
            return booking.BookingStatus switch
            {
                StatusConfirmed =>
                    "Lịch hẹn của anh/chị đã được Admin xác nhận. Showroom sẽ tiếp đón đúng khung giờ đã chốt.",
                StatusRescheduleRequested =>
                    "Khung giờ anh/chị chọn đã có khách khác đặt trước. Showroom sẽ liên hệ để đề nghị đổi sang khung giờ khác phù hợp.",
                StatusRejected =>
                    "Yêu cầu đặt lịch của anh/chị hiện chưa được showroom chấp nhận. Anh/chị vui lòng chờ thông báo tiếp theo từ Admin.",
                StatusCancelled =>
                    "Lịch hẹn này hiện đã bị hủy. Nếu cần, anh/chị có thể tạo yêu cầu đặt lịch mới.",
                _ =>
                    "Yêu cầu đặt lịch của anh/chị đã được ghi nhận và đang chờ Admin xác nhận. Lịch chỉ được chốt chính thức sau khi Admin duyệt."
            };
        }
    }
}
