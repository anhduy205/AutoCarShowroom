using AutoCarShowroom.Models;
using AutoCarShowroom.Services;
using AutoCarShowroom.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AutoCarShowroom.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ShowroomDbContext _context;
        private readonly BookingSchedulingService _bookingSchedulingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(
            ShowroomDbContext context,
            BookingSchedulingService bookingSchedulingService,
            ILogger<BookingsController> logger)
        {
            _context = context;
            _bookingSchedulingService = bookingSchedulingService;
            _logger = logger;
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
                    Car = BuildPurchaseSummary(car),
                    ServiceType = BookingWorkflow.ServiceViewing
                };

                PopulateServiceTypeOptions(model.ServiceType);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to open booking form for car {CarId}.", carId.Value);
                TempData["ErrorMessage"] = DatabaseIssueHelper.IsDatabaseConnectivityIssue(ex)
                    ? "Chưa thể mở biểu mẫu đặt lịch vì hệ thống chưa kết nối được cơ sở dữ liệu showroom. Anh/chị vui lòng kiểm tra SQL Server rồi thử lại giúp em."
                    : "Chưa thể mở biểu mẫu đặt lịch. Vui lòng thử lại sau.";
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
                model.AppointmentAt = BookingWorkflow.NormalizeSlot(model.AppointmentAt);

                ValidateServiceType(model.ServiceType);

                if (model.AppointmentAt <= DateTime.Now)
                {
                    ModelState.AddModelError(nameof(BookingCreateViewModel.AppointmentAt), "Ngày giờ hẹn phải lớn hơn thời điểm hiện tại.");
                }

                BookingSlotEvaluationResult? slotEvaluation = null;
                if (ModelState.IsValid)
                {
                    slotEvaluation = await _bookingSchedulingService.EvaluateAsync(model.AppointmentAt);

                    if (slotEvaluation.IsFull)
                    {
                        var suggestedSlotText = slotEvaluation.SuggestedAppointmentAt.HasValue
                            ? $" Khung giờ gần nhất còn trống là {slotEvaluation.SuggestedAppointmentAt.Value:HH:mm dd/MM/yyyy}."
                            : string.Empty;

                        ModelState.AddModelError(
                            nameof(BookingCreateViewModel.AppointmentAt),
                            $"Khung giờ {slotEvaluation.SlotStart:HH:mm dd/MM/yyyy} đã có khách đặt trước.{suggestedSlotText}");
                    }
                }

                if (!ModelState.IsValid)
                {
                    PopulateServiceTypeOptions(model.ServiceType);
                    return View(model);
                }

                var bookingCreation = await CreateBookingWithLockAsync(model);
                if (bookingCreation == null)
                {
                    TempData["ErrorMessage"] = "Mẫu xe này hiện không còn nhận đặt lịch.";
                    return RedirectToAction("Details", "Cars", new { id = model.CarId });
                }

                if (bookingCreation.Value.Booking == null || bookingCreation.Value.SlotEvaluation == null)
                {
                    var refreshedSlot = bookingCreation.Value.SlotEvaluation;
                    var suggestedSlotText = refreshedSlot?.SuggestedAppointmentAt.HasValue == true
                        ? $" Khung giờ gần nhất còn trống là {refreshedSlot.SuggestedAppointmentAt.Value:HH:mm dd/MM/yyyy}."
                        : string.Empty;

                    ModelState.AddModelError(
                        nameof(BookingCreateViewModel.AppointmentAt),
                        $"Khung giờ {model.AppointmentAt:HH:mm dd/MM/yyyy} vừa được khách khác giữ trước.{suggestedSlotText}");
                    PopulateServiceTypeOptions(model.ServiceType);
                    return View(model);
                }

                var booking = bookingCreation.Value.Booking;
                slotEvaluation = bookingCreation.Value.SlotEvaluation;

                TempData["SuccessMessage"] = "Đã ghi nhận yêu cầu đặt lịch của bạn.";
                TempData["BookingCustomerMessage"] = slotEvaluation.CustomerMessage;
                return RedirectToAction(nameof(Success), new { bookingCode = booking.BookingCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to create booking request for car {CarId}.", model.CarId);
                TempData["ErrorMessage"] = DatabaseIssueHelper.IsDatabaseConnectivityIssue(ex)
                    ? "Chưa thể tạo yêu cầu đặt lịch vì hệ thống chưa kết nối được cơ sở dữ liệu showroom. Anh/chị vui lòng kiểm tra SQL Server rồi thử lại giúp em."
                    : "Chưa thể tạo yêu cầu đặt lịch. Vui lòng thử lại sau.";
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
                    Booking = booking,
                    CustomerMessage = TempData["BookingCustomerMessage"] as string
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to display booking success page for booking code {BookingCode}.", bookingCode);
                TempData["ErrorMessage"] = DatabaseIssueHelper.IsDatabaseConnectivityIssue(ex)
                    ? "Chưa thể hiển thị thông tin lịch hẹn vì hệ thống chưa kết nối được cơ sở dữ liệu showroom. Anh/chị vui lòng kiểm tra SQL Server rồi thử lại giúp em."
                    : "Chưa thể hiển thị thông tin lịch hẹn.";
                return RedirectToAction("Index", "Cars");
            }
        }

        private void PopulateServiceTypeOptions(string? selectedServiceType)
        {
            ViewBag.ServiceTypes = new SelectList(
                BookingWorkflow.ServiceTypes.Select(type => new
                {
                    Value = type,
                    Text = type
                }),
                "Value",
                "Text",
                selectedServiceType);
        }

        private void ValidateServiceType(string? serviceType)
        {
            if (!BookingWorkflow.ServiceTypes.Contains(serviceType, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(BookingCreateViewModel.ServiceType), "Vui lòng chọn loại dịch vụ hợp lệ.");
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

        private async Task<(Booking? Booking, BookingSlotEvaluationResult? SlotEvaluation)?> CreateBookingWithLockAsync(
            BookingCreateViewModel model)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var car = await _context.Cars
                .FirstOrDefaultAsync(item => item.CarID == model.CarId);

            if (car == null || !OrderWorkflow.CanOrder(car))
            {
                return null;
            }

            var slotEvaluation = await _bookingSchedulingService.EvaluateAsync(model.AppointmentAt);
            if (slotEvaluation.IsFull)
            {
                return (null, slotEvaluation);
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
                ServiceType = model.ServiceType,
                AppointmentAt = slotEvaluation.SlotStart,
                Note = model.Note,
                BookingStatus = slotEvaluation.InitialStatus,
                AdminNote = slotEvaluation.AdminNote,
                CreatedAt = DateTime.Now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (booking, slotEvaluation);
        }
    }
}
