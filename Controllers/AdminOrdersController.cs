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
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(order => order.Items)
                .OrderByDescending(order => order.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(item => item.Items)
                .ThenInclude(item => item.Car)
                .FirstOrDefaultAsync(item => item.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(new AdminOrderDetailViewModel
            {
                Order = order,
                OrderStatusOptions = OrderWorkflow.OrderStatuses
            });
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

            var order = await _context.Orders
                .Include(item => item.Items)
                .ThenInclude(item => item.Car)
                .FirstOrDefaultAsync(item => item.OrderId == id);

            if (order == null)
            {
                return NotFound();
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
    }
}
