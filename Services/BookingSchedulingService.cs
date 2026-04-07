using AutoCarShowroom.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Services
{
    public sealed class BookingSchedulingService
    {
        private const int OpeningHour = 8;
        private const int ClosingHour = 18;

        private readonly ShowroomDbContext _context;

        public BookingSchedulingService(ShowroomDbContext context)
        {
            _context = context;
        }

        public async Task<BookingSlotEvaluationResult> EvaluateAsync(DateTime appointmentAt, int? excludeBookingId = null)
        {
            var slotStart = BookingWorkflow.NormalizeSlot(appointmentAt);
            var slotEnd = slotStart.AddHours(1);

            var existingRequests = await _context.Bookings
                .AsNoTracking()
                .Where(booking =>
                    booking.AppointmentAt >= slotStart &&
                    booking.AppointmentAt < slotEnd &&
                    BookingWorkflow.SlotCountingStatuses.Contains(booking.BookingStatus) &&
                    (!excludeBookingId.HasValue || booking.BookingId != excludeBookingId.Value))
                .CountAsync();

            var isFull = existingRequests >= BookingWorkflow.MaxRequestsPerSlot;
            var isBusy = !isFull && existingRequests >= BookingWorkflow.BusySlotThreshold;
            var suggestedSlot = isFull ? await FindNextAvailableSlotAsync(slotStart) : null;

            var customerMessage = isFull
                ? $"Khung giờ {slotStart:HH:mm dd/MM/yyyy} hiện đã đủ số lượng tiếp nhận. Yêu cầu sẽ được ghi nhận ở trạng thái đề nghị đổi giờ và cần chờ Admin xác nhận lại."
                : isBusy
                    ? $"Khung giờ {slotStart:HH:mm dd/MM/yyyy} hiện đã có nhiều khách đăng ký. Yêu cầu của khách vẫn được ghi nhận nhưng sẽ cần chờ Admin xác nhận."
                    : "Yêu cầu đặt lịch sẽ được ghi nhận ở trạng thái chờ xác nhận và cần Admin duyệt lại trước khi chốt lịch chính thức.";

            var adminNote = isFull
                ? suggestedSlot.HasValue
                    ? $"Khung giờ khách chọn đã đầy. Đề nghị đổi sang {suggestedSlot.Value:dd/MM/yyyy HH:mm}."
                    : "Khung giờ khách chọn đã đầy. Cần liên hệ khách để đề nghị đổi giờ."
                : isBusy
                    ? $"Khung giờ này hiện đã có {existingRequests} yêu cầu chờ/đã xác nhận. Cần Admin kiểm tra khả năng tiếp nhận thực tế."
                    : null;

            return new BookingSlotEvaluationResult
            {
                SlotStart = slotStart,
                ExistingRequests = existingRequests,
                IsBusy = isBusy,
                IsFull = isFull,
                SuggestedAppointmentAt = suggestedSlot,
                InitialStatus = isFull ? BookingWorkflow.StatusRescheduleRequested : BookingWorkflow.StatusPendingConfirmation,
                CustomerMessage = customerMessage,
                AdminNote = adminNote
            };
        }

        private async Task<DateTime?> FindNextAvailableSlotAsync(DateTime requestedSlot)
        {
            var candidate = requestedSlot.AddHours(1);

            for (var index = 0; index < 32; index++)
            {
                candidate = NormalizeBusinessSlot(candidate);
                var candidateEnd = candidate.AddHours(1);

                var count = await _context.Bookings
                    .AsNoTracking()
                    .Where(booking =>
                        booking.AppointmentAt >= candidate &&
                        booking.AppointmentAt < candidateEnd &&
                        BookingWorkflow.SlotCountingStatuses.Contains(booking.BookingStatus))
                    .CountAsync();

                if (count < BookingWorkflow.MaxRequestsPerSlot)
                {
                    return candidate;
                }

                candidate = candidate.AddHours(1);
            }

            return null;
        }

        private static DateTime NormalizeBusinessSlot(DateTime candidate)
        {
            var slot = BookingWorkflow.NormalizeSlot(candidate);

            if (slot.Hour < OpeningHour)
            {
                return new DateTime(slot.Year, slot.Month, slot.Day, OpeningHour, 0, 0, slot.Kind);
            }

            if (slot.Hour >= ClosingHour)
            {
                var nextDay = slot.Date.AddDays(1);
                return new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, OpeningHour, 0, 0, slot.Kind);
            }

            return slot;
        }
    }

    public sealed class BookingSlotEvaluationResult
    {
        public DateTime SlotStart { get; init; }

        public int ExistingRequests { get; init; }

        public bool IsBusy { get; init; }

        public bool IsFull { get; init; }

        public DateTime? SuggestedAppointmentAt { get; init; }

        public string InitialStatus { get; init; } = BookingWorkflow.StatusPendingConfirmation;

        public string CustomerMessage { get; init; } = string.Empty;

        public string? AdminNote { get; init; }
    }
}
