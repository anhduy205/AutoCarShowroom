using AutoCarShowroom.Models;
using AutoCarShowroom.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ShowroomDbContext _context;

        public BookingsController(ShowroomDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create(int? carId)
        {
            if (!carId.HasValue)
            {
                return RedirectToAction("Index", "Cars");
            }

            try
            {
                var car = await LoadBookableCarAsync(carId.Value);

                if (car == null)
                {
                    TempData["ErrorMessage"] = "Mẫu xe này hiện không còn nhận đặt lịch.";
                    return RedirectToAction("Details", "Cars", new { id = carId.Value });
                }

                var model = new BookingCreateViewModel
                {
                    CarId = car.CarID,
                    Car = BuildPurchaseSummary(car)
                };

                return View(model);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể mở biểu mẫu đặt lịch. Vui lòng thử lại sau.";
                return RedirectToAction("Details", "Cars", new { id = carId.Value });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingCreateViewModel model)
        {
            try
            {
                var car = await LoadBookableCarAsync(model.CarId);

                if (car == null)
                {
                    TempData["ErrorMessage"] = "Mẫu xe này hiện không còn nhận đặt lịch.";
                    return RedirectToAction("Details", "Cars", new { id = model.CarId });
                }

                model.Car = BuildPurchaseSummary(car);

                if (model.AppointmentAt <= DateTime.Now)
                {
                    ModelState.AddModelError(nameof(BookingCreateViewModel.AppointmentAt), "Ngày giờ hẹn phải lớn hơn thời điểm hiện tại.");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var booking = new Booking
                {
                    BookingCode = await GenerateBookingCodeAsync(),
                    CarId = car.CarID,
                    CarName = car.CarName,
                    CarImage = car.Image,
                    QuotedPrice = car.Price,
                    CustomerName = model.CustomerName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    AppointmentAt = model.AppointmentAt,
                    Note = model.Note,
                    BookingStatus = BookingWorkflow.StatusNew,
                    CreatedAt = DateTime.Now
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã gửi lịch hẹn thành công.";
                return RedirectToAction(nameof(Success), new { bookingCode = booking.BookingCode });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể tạo lịch hẹn. Vui lòng thử lại sau.";
                return RedirectToAction("Details", "Cars", new { id = model.CarId });
            }
        }

        public async Task<IActionResult> Success(string? bookingCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bookingCode))
                {
                    return RedirectToAction("Index", "Cars");
                }

                var booking = await _context.Bookings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item => item.BookingCode == bookingCode);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch hẹn bạn vừa tạo.";
                    return RedirectToAction("Index", "Cars");
                }

                return View(new BookingSuccessViewModel
                {
                    Booking = booking
                });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể hiển thị thông tin lịch hẹn.";
                return RedirectToAction("Index", "Cars");
            }
        }

        private async Task<Car?> LoadBookableCarAsync(int carId)
        {
            var car = await _context.Cars
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.CarID == carId);

            return car != null && OrderWorkflow.CanOrder(car) ? car : null;
        }

        private static PurchaseCarSummaryViewModel BuildPurchaseSummary(Car car)
        {
            return new PurchaseCarSummaryViewModel
            {
                CarId = car.CarID,
                CarName = car.CarName,
                Brand = car.Brand,
                ModelName = car.ModelName,
                Image = car.Image,
                Status = car.Status,
                Price = car.Price,
                CanPurchase = OrderWorkflow.CanOrder(car)
            };
        }

        private async Task<string> GenerateBookingCodeAsync()
        {
            while (true)
            {
                var bookingCode = $"BK-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..24];
                var exists = await _context.Bookings.AnyAsync(item => item.BookingCode == bookingCode);

                if (!exists)
                {
                    return bookingCode;
                }
            }
        }
    }
}
