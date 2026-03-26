using AutoCarShowroom.Extensions;
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
            try
            {
                var selection = await ResolveCheckoutSelectionAsync(carId);

                if (!selection.Success)
                {
                    TempData["ErrorMessage"] = selection.ErrorMessage;
                    return selection.RedirectResult!;
                }

                var model = new CheckoutViewModel
                {
                    IsBuyNow = carId.HasValue,
                    BuyNowCarId = carId,
                    Items = await BuildCartItemsAsync(selection.Cars)
                };

                PopulatePaymentOptions(model.PaymentMethod);
                return View(model);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Trang thanh toán tạm thời chưa sẵn sàng vì hệ thống đơn hàng chưa kết nối được cơ sở dữ liệu hoặc chưa cập nhật đủ bảng.";
                return RedirectToCheckoutFallback(carId);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            try
            {
                var selection = await ResolveCheckoutSelectionAsync(model.BuyNowCarId);

                if (!selection.Success)
                {
                    TempData["ErrorMessage"] = selection.ErrorMessage;
                    return selection.RedirectResult!;
                }

                model.Items = await BuildCartItemsAsync(selection.Cars);

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
                    TotalAmount = model.TotalAmount,
                    Items = selection.Cars
                        .Select(car => new OrderItem
                        {
                            CarId = car.CarID,
                            CarName = car.CarName,
                            CarImage = car.Image,
                            UnitPrice = car.Price
                        })
                        .ToList()
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                var remainingCarIds = HttpContext.Session
                    .GetCartCarIds()
                    .Except(selection.Cars.Select(car => car.CarID))
                    .ToList();

                HttpContext.Session.SetCartCarIds(remainingCarIds);

                TempData["SuccessMessage"] = "Đã tạo đơn đặt xe thành công.";
                return RedirectToAction(nameof(Success), new { orderCode = order.OrderCode });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể tạo đơn hàng vì hệ thống đơn hàng chưa sẵn sàng. Vui lòng kiểm tra lại kết nối cơ sở dữ liệu.";
                return RedirectToCheckoutFallback(model.BuyNowCarId);
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
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng bạn vừa tạo.";
                    return RedirectToAction("Index", "Cars");
                }

                return View(new OrderSuccessViewModel
                {
                    Order = order,
                    Items = order.Items
                        .OrderBy(item => item.OrderItemId)
                        .ToList()
                });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Chưa thể hiển thị thông tin đơn hàng vì cơ sở dữ liệu chưa sẵn sàng.";
                return RedirectToAction("Index", "Cars");
            }
        }

        private void PopulatePaymentOptions(string? selectedPaymentMethod)
        {
            ViewBag.PaymentMethods = new SelectList(OrderWorkflow.PaymentMethods, selectedPaymentMethod);
        }

        private IActionResult RedirectToCheckoutFallback(int? buyNowCarId)
        {
            if (buyNowCarId.HasValue)
            {
                return RedirectToAction("Details", "Cars", new { id = buyNowCarId.Value });
            }

            return RedirectToAction("Index", "Cart");
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

        private async Task<IReadOnlyList<CartItemViewModel>> BuildCartItemsAsync(IReadOnlyCollection<Car> cars)
        {
            var carIds = cars.Select(car => car.CarID).ToList();
            var lockedCarIds = await GetLockedCarIdsAsync(carIds);

            return cars
                .Select(car => new CartItemViewModel
                {
                    CarId = car.CarID,
                    CarName = car.CarName,
                    Brand = car.Brand,
                    ModelName = car.ModelName,
                    Image = car.Image,
                    Status = car.Status,
                    Price = car.Price,
                    CanOrder = OrderWorkflow.CanOrder(car) && !lockedCarIds.Contains(car.CarID),
                    AvailabilityMessage = GetAvailabilityMessage(car, lockedCarIds.Contains(car.CarID))
                })
                .ToList();
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

        private async Task<CheckoutSelectionResult> ResolveCheckoutSelectionAsync(int? buyNowCarId)
        {
            if (buyNowCarId.HasValue)
            {
                var car = await _context.Cars
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item => item.CarID == buyNowCarId.Value);

                if (car == null)
                {
                    return CheckoutSelectionResult.Fail(
                        "Không tìm thấy mẫu xe cần thanh toán.",
                        RedirectToAction("Index", "Cars"));
                }

                if (!OrderWorkflow.CanOrder(car))
                {
                    return CheckoutSelectionResult.Fail(
                        "Mẫu xe này hiện không còn sẵn sàng để đặt.",
                        RedirectToAction("Details", "Cars", new { id = buyNowCarId.Value }));
                }

                var lockedCarIds = await GetLockedCarIdsAsync([buyNowCarId.Value]);

                if (lockedCarIds.Contains(buyNowCarId.Value))
                {
                    return CheckoutSelectionResult.Fail(
                        "Mẫu xe này đang được xử lý trong một đơn hàng khác.",
                        RedirectToAction("Details", "Cars", new { id = buyNowCarId.Value }));
                }

                return CheckoutSelectionResult.Ok([car]);
            }

            var sessionCarIds = HttpContext.Session.GetCartCarIds();

            if (sessionCarIds.Count == 0)
            {
                return CheckoutSelectionResult.Fail(
                    "Giỏ hàng của bạn đang trống.",
                    RedirectToAction("Index", "Cart"));
            }

            var cars = await _context.Cars
                .AsNoTracking()
                .Where(car => sessionCarIds.Contains(car.CarID))
                .ToListAsync();

            var orderedCars = sessionCarIds
                .Select(carId => cars.FirstOrDefault(car => car.CarID == carId))
                .Where(car => car != null)
                .Select(car => car!)
                .ToList();

            if (orderedCars.Count != sessionCarIds.Count)
            {
                HttpContext.Session.SetCartCarIds(orderedCars.Select(car => car.CarID));

                return CheckoutSelectionResult.Fail(
                    "Giỏ hàng đã được cập nhật vì có xe không còn tồn tại.",
                    RedirectToAction("Index", "Cart"));
            }

            var lockedIds = await GetLockedCarIdsAsync(orderedCars.Select(car => car.CarID).ToList());
            var hasUnavailableCars = orderedCars.Any(car => !OrderWorkflow.CanOrder(car) || lockedIds.Contains(car.CarID));

            if (hasUnavailableCars)
            {
                return CheckoutSelectionResult.Fail(
                    "Một hoặc nhiều mẫu xe trong giỏ hiện không thể thanh toán. Vui lòng kiểm tra lại giỏ hàng.",
                    RedirectToAction("Index", "Cart"));
            }

            return CheckoutSelectionResult.Ok(orderedCars);
        }

        private static string GetAvailabilityMessage(Car car, bool isLocked)
        {
            if (!OrderWorkflow.CanOrder(car))
            {
                return "Xe này hiện không còn sẵn sàng để đặt.";
            }

            if (isLocked)
            {
                return "Xe này đang được xử lý trong một đơn hàng khác.";
            }

            return string.Empty;
        }

        private sealed class CheckoutSelectionResult
        {
            public bool Success { get; private init; }

            public List<Car> Cars { get; private init; } = [];

            public string ErrorMessage { get; private init; } = string.Empty;

            public IActionResult? RedirectResult { get; private init; }

            public static CheckoutSelectionResult Ok(List<Car> cars)
            {
                return new CheckoutSelectionResult
                {
                    Success = true,
                    Cars = cars
                };
            }

            public static CheckoutSelectionResult Fail(string errorMessage, IActionResult redirectResult)
            {
                return new CheckoutSelectionResult
                {
                    Success = false,
                    ErrorMessage = errorMessage,
                    RedirectResult = redirectResult
                };
            }
        }
    }
}
