using AutoCarShowroom.Models;
using AutoCarShowroom.Services;
using AutoCarShowroom.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Controllers
{
    [Authorize(Roles = InternalAccess.BackOfficeRoles)]
    public class AdminBookingsController : Controller
    {
        private readonly ShowroomDbContext _context;
        private readonly BookingSchedulingService _bookingSchedulingService;
        private readonly BookingEmailService _bookingEmailService;

        public AdminBookingsController(
            ShowroomDbContext context,
            BookingSchedulingService bookingSchedulingService,
            BookingEmailService bookingEmailService)
        {
            _context = context;
            _bookingSchedulingService = bookingSchedulingService;
            _bookingEmailService = bookingEmailService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var bookings = await _context.Bookings
                    .AsNoTracking()
                    .OrderBy(item => item.AppointmentAt)
                    .ThenByDescending(item => item.CreatedAt)
                    .ToListAsync();

                return View(bookings);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Danh sách lịch hẹn tạm thời chưa sẵn sàng.";
                return View(Enumerable.Empty<Booking>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(item => item.Car)
                    .FirstOrDefaultAsync(item => item.BookingId == id);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch hẹn cần xem.";
                    return RedirectToAction(nameof(Index));
                }

                return View(new AdminBookingDetailViewModel
                {
                    Booking = booking,
                    BookingStatusOptions = BookingWorkflow.Statuses
                });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể mở chi tiết lịch hẹn.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(item => item.Car)
                    .FirstOrDefaultAsync(item => item.BookingId == id);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch hẹn cần xác nhận.";
                    return RedirectToAction(nameof(Index));
                }

                var slotConflictMessage = await GetSlotConflictMessageAsync(booking);
                if (!string.IsNullOrWhiteSpace(slotConflictMessage))
                {
                    TempData["ErrorMessage"] = slotConflictMessage;
                    return RedirectToAction(nameof(Details), new { id });
                }

                booking.BookingStatus = BookingWorkflow.StatusConfirmed;
                booking.AdminNote = string.IsNullOrWhiteSpace(booking.AdminNote)
                    ? "Admin đã xác nhận lịch hẹn."
                    : booking.AdminNote;

                await _context.SaveChangesAsync();
                var emailResult = await _bookingEmailService.SendBookingConfirmedAsync(booking);
                TempData["SuccessMessage"] = BuildConfirmationSuccessMessage(emailResult);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể xác nhận lịch hẹn.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(item => item.BookingId == id);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch hẹn cần xóa.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã xóa lịch hẹn.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Chưa thể xóa lịch hẹn vì dữ liệu này đang được sử dụng ở nơi khác.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể xóa lịch hẹn.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string bookingStatus, string? adminNote)
        {
            if (!BookingWorkflow.Statuses.Contains(bookingStatus, StringComparer.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Trạng thái lịch hẹn không hợp lệ.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var booking = await _context.Bookings
                    .Include(item => item.Car)
                    .FirstOrDefaultAsync(item => item.BookingId == id);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch hẹn cần cập nhật.";
                    return RedirectToAction(nameof(Index));
                }

                if (string.Equals(bookingStatus, BookingWorkflow.StatusConfirmed, StringComparison.OrdinalIgnoreCase))
                {
                    var slotConflictMessage = await GetSlotConflictMessageAsync(booking);
                    if (!string.IsNullOrWhiteSpace(slotConflictMessage))
                    {
                        TempData["ErrorMessage"] = slotConflictMessage;
                        return RedirectToAction(nameof(Details), new { id });
                    }
                }

                var wasConfirmed = string.Equals(
                    booking.BookingStatus,
                    BookingWorkflow.StatusConfirmed,
                    StringComparison.OrdinalIgnoreCase);

                booking.BookingStatus = bookingStatus;
                booking.AdminNote = string.IsNullOrWhiteSpace(adminNote)
                    ? booking.AdminNote
                    : adminNote.Trim();

                if (string.Equals(bookingStatus, BookingWorkflow.StatusConfirmed, StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrWhiteSpace(booking.AdminNote))
                {
                    booking.AdminNote = "Admin đã xác nhận lịch hẹn.";
                }

                if (string.Equals(bookingStatus, BookingWorkflow.StatusSold, StringComparison.OrdinalIgnoreCase) &&
                    booking.Car != null)
                {
                    booking.Car.Status = OrderWorkflow.CarStatusSold;
                }

                await _context.SaveChangesAsync();

                if (!wasConfirmed &&
                    string.Equals(bookingStatus, BookingWorkflow.StatusConfirmed, StringComparison.OrdinalIgnoreCase))
                {
                    var emailResult = await _bookingEmailService.SendBookingConfirmedAsync(booking);
                    TempData["SuccessMessage"] = BuildConfirmationSuccessMessage(emailResult);
                }
                else
                {
                    TempData["SuccessMessage"] = "Đã cập nhật lịch hẹn.";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể cập nhật lịch hẹn.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<string?> GetSlotConflictMessageAsync(Booking booking)
        {
            var slotEvaluation = await _bookingSchedulingService.EvaluateAsync(booking.AppointmentAt, booking.BookingId);
            if (!slotEvaluation.IsFull)
            {
                return null;
            }

            var suggestedSlotText = slotEvaluation.SuggestedAppointmentAt.HasValue
                ? $" Có thể đổi sang {slotEvaluation.SuggestedAppointmentAt.Value:HH:mm dd/MM/yyyy}."
                : string.Empty;

            return $"Không thể xác nhận lịch lúc {slotEvaluation.SlotStart:HH:mm dd/MM/yyyy} vì đã có lịch khác trùng giờ.{suggestedSlotText}";
        }

        private static string BuildConfirmationSuccessMessage(EmailSendResult emailResult)
        {
            if (emailResult.Succeeded)
            {
                return "Đã xác nhận lịch hẹn và gửi email thông báo cho khách.";
            }

            if (emailResult.WasSkipped)
            {
                return $"Đã xác nhận lịch hẹn. {emailResult.Message}";
            }

            return $"Đã xác nhận lịch hẹn nhưng {emailResult.Message?.ToLowerInvariant() ?? "chưa gửi được email thông báo cho khách."}";
        }
    }
}
