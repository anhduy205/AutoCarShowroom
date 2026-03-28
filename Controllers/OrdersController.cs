using AutoCarShowroom.Models;
using AutoCarShowroom.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ShowroomDbContext _context;

        public OrdersController(ShowroomDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Checkout(int? carId)
        {
            if (!carId.HasValue)
            {
                return RedirectToAction("Index", "Cars");
            }

            try
            {
                var car = await LoadPurchasableCarAsync(carId.Value);

                if (car == null)
                {
                    TempData["ErrorMessage"] = "Mẫu xe này hiện không sẵn sàng để mua.";
                    return RedirectToAction("Details", "Cars", new { id = carId.Value });
                }

                var model = new CheckoutViewModel
                {
                    CarId = car.CarID,
                    Car = await BuildPurchaseSummaryAsync(car)
                };

                PopulatePaymentOptions(model.PaymentMethod);
                return View(model);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Trang thanh toán tạm thời chưa sẵn sàng. Vui lòng thử lại sau.";
                return RedirectToAction("Details", "Cars", new { id = carId.Value });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            try
            {
                var car = await LoadPurchasableCarAsync(model.CarId);

                if (car == null)
                {
                    TempData["ErrorMessage"] = "Mẫu xe này hiện không sẵn sàng để mua.";
                    return RedirectToAction("Details", "Cars", new { id = model.CarId });
                }

                model.Car = await BuildPurchaseSummaryAsync(car);

                if (!ModelState.IsValid)
                {
                    PopulatePaymentOptions(model.PaymentMethod);
                    return View(model);
                }

                var order = new Order
                {
                    OrderCode = await GenerateOrderCodeAsync(),
                    CustomerName = model.CustomerName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Address = model.Address,
                    Note = model.Note,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = OrderWorkflow.PaymentStatusPaid,
                    OrderStatus = OrderWorkflow.OrderStatusPaid,
                    CreatedAt = DateTime.Now,
                    TotalAmount = car.Price,
                    Items =
                    [
                        new OrderItem
                        {
                            CarId = car.CarID,
                            CarName = car.CarName,
                            CarImage = car.Image,
                            UnitPrice = car.Price
                        }
                    ]
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã tạo đơn mua xe thành công.";
                return RedirectToAction(nameof(Success), new { orderCode = order.OrderCode });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể tạo đơn mua xe. Vui lòng kiểm tra lại dữ liệu và thử lại.";
                return RedirectToAction("Details", "Cars", new { id = model.CarId });
            }
        }

        public async Task<IActionResult> Success(string? orderCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderCode))
                {
                    return RedirectToAction("Index", "Cars");
                }

                var order = await _context.Orders
                    .AsNoTracking()
                    .Include(item => item.Items)
                    .FirstOrDefaultAsync(item => item.OrderCode == orderCode);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn mua xe bạn vừa tạo.";
                    return RedirectToAction("Index", "Cars");
                }

                return View(new OrderSuccessViewModel
                {
                    Order = order,
                    Items = order.Items.OrderBy(item => item.OrderItemId).ToList()
                });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể hiển thị thông tin đơn mua xe.";
                return RedirectToAction("Index", "Cars");
            }
        }

        private void PopulatePaymentOptions(string? selectedPaymentMethod)
        {
            ViewBag.PaymentMethods = new SelectList(OrderWorkflow.PaymentMethods, selectedPaymentMethod);
        }

        private async Task<Car?> LoadPurchasableCarAsync(int carId)
        {
            var car = await _context.Cars
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.CarID == carId);

            if (car == null || !OrderWorkflow.CanOrder(car))
            {
                return null;
            }

            var lockedCarIds = await GetLockedCarIdsAsync([carId]);
            return lockedCarIds.Contains(carId) ? null : car;
        }

        private async Task<PurchaseCarSummaryViewModel> BuildPurchaseSummaryAsync(Car car)
        {
            var lockedCarIds = await GetLockedCarIdsAsync([car.CarID]);
            var isLocked = lockedCarIds.Contains(car.CarID);

            return new PurchaseCarSummaryViewModel
            {
                CarId = car.CarID,
                CarName = car.CarName,
                Brand = car.Brand,
                ModelName = car.ModelName,
                Image = car.Image,
                Status = car.Status,
                Price = car.Price,
                CanPurchase = OrderWorkflow.CanOrder(car) && !isLocked,
                AvailabilityMessage = GetAvailabilityMessage(car, isLocked)
            };
        }

        private async Task<HashSet<int>> GetLockedCarIdsAsync(IReadOnlyCollection<int> carIds)
        {
            if (carIds.Count == 0)
            {
                return [];
            }

            var lockedIds = await _context.OrderItems
                .AsNoTracking()
                .Where(item =>
                    carIds.Contains(item.CarId) &&
                    OrderWorkflow.LockingOrderStatuses.Contains(item.Order.OrderStatus))
                .Select(item => item.CarId)
                .Distinct()
                .ToListAsync();

            return lockedIds.ToHashSet();
        }

        private async Task<string> GenerateOrderCodeAsync()
        {
            while (true)
            {
                var orderCode = $"DH-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..24];

                var exists = await _context.Orders.AnyAsync(order => order.OrderCode == orderCode);

                if (!exists)
                {
                    return orderCode;
                }
            }
        }

        private static string GetAvailabilityMessage(Car car, bool isLocked)
        {
            if (!OrderWorkflow.CanOrder(car))
            {
                return "Xe này hiện không còn sẵn sàng để mua.";
            }

            if (isLocked)
            {
                return "Xe này đang được xử lý trong một đơn mua khác.";
            }

            return string.Empty;
        }
    }
}
