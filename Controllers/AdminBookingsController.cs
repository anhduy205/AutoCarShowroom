using AutoCarShowroom.Models;
using AutoCarShowroom.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBookingsController : Controller
    {
        private readonly ShowroomDbContext _context;

        public AdminBookingsController(ShowroomDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var bookings = await _context.Bookings
                    .AsNoTracking()
                    .OrderByDescending(item => item.CreatedAt)
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

                booking.BookingStatus = bookingStatus;
                booking.AdminNote = adminNote;

                if (string.Equals(bookingStatus, BookingWorkflow.StatusSold, StringComparison.OrdinalIgnoreCase) &&
                    booking.Car != null)
                {
                    booking.Car.Status = OrderWorkflow.CarStatusSold;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã cập nhật lịch hẹn.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể cập nhật lịch hẹn.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
