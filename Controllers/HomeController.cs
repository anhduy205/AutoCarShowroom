using System.Diagnostics;
using AutoCarShowroom.Models;
using AutoCarShowroom.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ShowroomDbContext _context;

        public HomeController(ILogger<HomeController> logger, ShowroomDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var visibleCarsQuery = _context.Cars.AsNoTracking().AsQueryable();

                if (!User.IsInRole("Admin"))
                {
                    visibleCarsQuery = visibleCarsQuery.Where(car => OrderWorkflow.PurchasableCarStatuses.Contains(car.Status));
                }

                var summary = await visibleCarsQuery
                    .GroupBy(_ => 1)
                    .Select(group => new
                    {
                        TotalCars = group.Count(),
                        AveragePrice = group.Average(car => car.Price),
                        NewestModelYear = group.Max(car => car.Year)
                    })
                    .FirstOrDefaultAsync();

                var featuredCars = await visibleCarsQuery
                    .OrderByDescending(car => car.Year)
                    .ThenByDescending(car => car.Price)
                    .Take(6)
                    .ToListAsync();

                var viewModel = new HomeViewModel
                {
                    FeaturedCars = featuredCars,
                    TotalCars = summary?.TotalCars ?? 0,
                    AveragePrice = summary?.AveragePrice ?? 0m,
                    NewestModelYear = summary?.NewestModelYear ?? 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể tải dữ liệu cho trang chủ showroom.");
                ViewData["LoadError"] = "Chưa thể tải dữ liệu showroom. Vui lòng kiểm tra kết nối SQL Server và migration.";
                return View(new HomeViewModel());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
