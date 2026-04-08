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
            var suggestedSlot = isFull ? await FindNextAvailableSlotAsync(slotStart) : null;

            var customerMessage = isFull
                ? suggestedSlot.HasValue
                    ? $"Khung giờ {slotStart:HH:mm dd/MM/yyyy} đã có khách khác đặt trước. Anh/chị vui lòng chọn giờ khác, hoặc tham khảo {suggestedSlot.Value:HH:mm dd/MM/yyyy}."
                    : $"Khung giờ {slotStart:HH:mm dd/MM/yyyy} đã có khách khác đặt trước. Anh/chị vui lòng chọn giờ khác."
                : "Yêu cầu đặt lịch sẽ được ghi nhận ở trạng thái chờ xác nhận và cần Admin duyệt lại trước khi chốt lịch chính thức.";

            var adminNote = isFull
                ? suggestedSlot.HasValue
                    ? $"Khung giờ khách chọn đã có lịch khác. Có thể đề xuất đổi sang {suggestedSlot.Value:dd/MM/yyyy HH:mm}."
                    : "Khung giờ khách chọn đã có lịch khác. Cần liên hệ khách để đổi giờ."
                : null;

            return new BookingSlotEvaluationResult
            {
                SlotStart = slotStart,
                ExistingRequests = existingRequests,
                IsBusy = false,
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
