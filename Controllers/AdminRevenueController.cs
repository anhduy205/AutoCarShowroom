using AutoCarShowroom.Models;
using AutoCarShowroom.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Controllers
{
    [Authorize(Policy = InternalAccess.RevenuePolicy)]
    public class AdminRevenueController : Controller
    {
        private readonly ShowroomDbContext _context;

        public AdminRevenueController(ShowroomDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await BuildDashboardViewModelAsync());
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Trang doanh thu tạm thời chưa sẵn sàng.";
                PopulateEmptyReferenceOptions();
                return View(new RevenueDashboardViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RevenueRecordFormViewModel form)
        {
            try
            {
                ValidateRevenueForm(form);

                if (!ModelState.IsValid)
                {
                    var invalidModel = await BuildDashboardViewModelAsync();
                    invalidModel.Form = form;
                    return View("Index", invalidModel);
                }

                var record = new RevenueRecord
                {
                    Amount = form.Amount,
                    ReceivedAt = form.ReceivedAt,
                    SourceType = form.SourceType,
                    OrderId = form.SourceType == RevenueWorkflow.SourceTypeOrder ? form.OrderId : null,
                    BookingId = form.SourceType == RevenueWorkflow.SourceTypeBooking ? form.BookingId : null,
                    Note = form.Note
                };

                _context.RevenueRecords.Add(record);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã ghi nhận doanh thu thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể lưu doanh thu. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<RevenueDashboardViewModel> BuildDashboardViewModelAsync()
        {
            var records = await _context.RevenueRecords
                .AsNoTracking()
                .Include(item => item.Order)
                .Include(item => item.Booking)
                .OrderByDescending(item => item.ReceivedAt)
                .ThenByDescending(item => item.RevenueRecordId)
                .ToListAsync();

            PopulateReferenceOptions();

            return new RevenueDashboardViewModel
            {
                TotalRevenue = records.Sum(item => item.Amount),
                TotalTransactions = records.Count,
                OrderRevenueCount = records.Count(item => item.SourceType == RevenueWorkflow.SourceTypeOrder),
                BookingRevenueCount = records.Count(item => item.SourceType == RevenueWorkflow.SourceTypeBooking),
                Records = records
            };
        }

        private void PopulateReferenceOptions()
        {
            var orderOptions = _context.Orders
                .AsNoTracking()
                .OrderByDescending(item => item.CreatedAt)
                .Select(item => new
                {
                    item.OrderId,
                    Label = item.OrderCode + " - " + item.CustomerName
                })
                .ToList();

            var bookingOptions = _context.Bookings
                .AsNoTracking()
                .OrderByDescending(item => item.CreatedAt)
                .Select(item => new
                {
                    item.BookingId,
                    Label = item.BookingCode + " - " + item.CustomerName
                })
                .ToList();

            ViewBag.SourceTypes = new SelectList(RevenueWorkflow.SourceTypes);
            ViewBag.OrderOptions = new SelectList(orderOptions, "OrderId", "Label");
            ViewBag.BookingOptions = new SelectList(bookingOptions, "BookingId", "Label");
        }

        private void PopulateEmptyReferenceOptions()
        {
            ViewBag.SourceTypes = new SelectList(RevenueWorkflow.SourceTypes);
            ViewBag.OrderOptions = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.BookingOptions = new SelectList(Enumerable.Empty<SelectListItem>());
        }

        private void ValidateRevenueForm(RevenueRecordFormViewModel form)
        {
            if (!RevenueWorkflow.SourceTypes.Contains(form.SourceType, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(RevenueRecordFormViewModel.SourceType), "Nguồn doanh thu không hợp lệ.");
            }

            if (form.ReceivedAt == default)
            {
                ModelState.AddModelError(nameof(RevenueRecordFormViewModel.ReceivedAt), "Vui lòng chọn ngày ghi nhận.");
            }

            if (string.Equals(form.SourceType, RevenueWorkflow.SourceTypeOrder, StringComparison.OrdinalIgnoreCase) && !form.OrderId.HasValue)
            {
                ModelState.AddModelError(nameof(RevenueRecordFormViewModel.OrderId), "Vui lòng chọn đơn mua xe.");
            }

            if (string.Equals(form.SourceType, RevenueWorkflow.SourceTypeBooking, StringComparison.OrdinalIgnoreCase) && !form.BookingId.HasValue)
            {
                ModelState.AddModelError(nameof(RevenueRecordFormViewModel.BookingId), "Vui lòng chọn lịch hẹn.");
            }
        }
    }
}
