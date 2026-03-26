using AutoCarShowroom.Models;
using AutoCarShowroom.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly ShowroomDbContext _context;

        public AdminOrdersController(ShowroomDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _context.Orders
                    .AsNoTracking()
                    .Include(order => order.Items)
                    .OrderByDescending(order => order.CreatedAt)
                    .ToListAsync();

                return View(orders);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Danh sách đơn hàng tạm thời chưa sẵn sàng vì cơ sở dữ liệu chưa kết nối hoặc chưa cập nhật đủ bảng.";
                return View(Enumerable.Empty<Order>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(item => item.Items)
                    .ThenInclude(item => item.Car)
                    .FirstOrDefaultAsync(item => item.OrderId == id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng cần xem.";
                    return RedirectToAction(nameof(Index));
                }

                return View(new AdminOrderDetailViewModel
                {
                    Order = order,
                    OrderStatusOptions = OrderWorkflow.OrderStatuses
                });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể mở chi tiết đơn hàng vì hệ thống đơn hàng chưa sẵn sàng.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string orderStatus)
        {
            if (!OrderWorkflow.OrderStatuses.Contains(orderStatus, StringComparer.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Trạng thái đơn hàng không hợp lệ.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var order = await _context.Orders
                    .Include(item => item.Items)
                    .ThenInclude(item => item.Car)
                    .FirstOrDefaultAsync(item => item.OrderId == id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng cần cập nhật.";
                    return RedirectToAction(nameof(Index));
                }

                order.OrderStatus = orderStatus;

                if (string.Equals(orderStatus, OrderWorkflow.OrderStatusCompleted, StringComparison.OrdinalIgnoreCase))
                {
                    order.PaymentStatus = OrderWorkflow.PaymentStatusPaid;

                    foreach (var item in order.Items)
                    {
                        if (item.Car != null)
                        {
                            item.Car.Status = OrderWorkflow.CarStatusSold;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã cập nhật trạng thái đơn hàng.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể cập nhật đơn hàng vì cơ sở dữ liệu chưa sẵn sàng.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
