using AutoCarShowroom.Extensions;
using AutoCarShowroom.Models;
using AutoCarShowroom.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Controllers
{
    public class CartController : Controller
    {
        private readonly ShowroomDbContext _context;

        public CartController(ShowroomDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var carIds = HttpContext.Session.GetCartCarIds();
            var items = await BuildCartItemsAsync(carIds, cleanupMissingCars: true);

            return View(new CartViewModel
            {
                Items = items
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int carId, string? returnUrl = null)
        {
            var car = await _context.Cars
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.CarID == carId);

            if (car == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mẫu xe bạn muốn thêm vào giỏ.";
                return RedirectToAction("Index", "Cars");
            }

            if (!OrderWorkflow.CanOrder(car))
            {
                TempData["ErrorMessage"] = "Mẫu xe này hiện không còn sẵn sàng để đặt.";
                return RedirectToLocalOrDetails(returnUrl, carId);
            }

            var lockedCarIds = await GetLockedCarIdsAsync([carId]);

            if (lockedCarIds.Contains(carId))
            {
                TempData["ErrorMessage"] = "Mẫu xe này đang được xử lý trong một đơn hàng khác.";
                return RedirectToLocalOrDetails(returnUrl, carId);
            }

            var added = HttpContext.Session.AddCarToCart(carId);
            TempData["SuccessMessage"] = added
                ? $"Đã thêm {car.CarName} vào giỏ hàng."
                : "Mẫu xe này đã có trong giỏ hàng của bạn.";

            return RedirectToLocalOrDetails(returnUrl, carId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int carId)
        {
            var removed = HttpContext.Session.RemoveCarFromCart(carId);

            if (removed)
            {
                TempData["SuccessMessage"] = "Đã xóa mẫu xe khỏi giỏ hàng.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            HttpContext.Session.ClearCart();
            TempData["SuccessMessage"] = "Đã làm trống giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<CartItemViewModel>> BuildCartItemsAsync(IReadOnlyCollection<int> carIds, bool cleanupMissingCars)
        {
            if (carIds.Count == 0)
            {
                return [];
            }

            var cars = await _context.Cars
                .AsNoTracking()
                .Where(car => carIds.Contains(car.CarID))
                .ToListAsync();

            var orderedCars = carIds
                .Select(carId => cars.FirstOrDefault(car => car.CarID == carId))
                .Where(car => car != null)
                .Select(car => car!)
                .ToList();

            if (cleanupMissingCars && orderedCars.Count != carIds.Count)
            {
                HttpContext.Session.SetCartCarIds(orderedCars.Select(car => car.CarID));
            }

            var lockedCarIds = await GetLockedCarIdsAsync(orderedCars.Select(car => car.CarID).ToList());

            return orderedCars
                .Select(car => MapCartItem(car, lockedCarIds.Contains(car.CarID)))
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

        private static CartItemViewModel MapCartItem(Car car, bool isLocked)
        {
            var canOrder = OrderWorkflow.CanOrder(car) && !isLocked;

            return new CartItemViewModel
            {
                CarId = car.CarID,
                CarName = car.CarName,
                Brand = car.Brand,
                ModelName = car.ModelName,
                Image = car.Image,
                Status = car.Status,
                Price = car.Price,
                CanOrder = canOrder,
                AvailabilityMessage = GetAvailabilityMessage(car, isLocked)
            };
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

        private IActionResult RedirectToLocalOrDetails(string? returnUrl, int carId)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", "Cars", new { id = carId });
        }
    }
}
