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

                var visibleCars = await visibleCarsQuery.ToListAsync();

                var featuredLines = CarShowcaseMapper.BuildLineCards(visibleCars, "year_desc")
                    .Take(6)
                    .ToList();

                var viewModel = new HomeViewModel
                {
                    HeroLine = featuredLines.FirstOrDefault(),
                    FeaturedLines = featuredLines,
                    TotalCars = visibleCars.Count,
                    TotalLines = featuredLines.Count == 0
                        ? 0
                        : visibleCars
                            .GroupBy(car => new { car.Brand, car.ModelName })
                            .Count(),
                    AveragePrice = visibleCars.Count == 0 ? 0m : visibleCars.Average(car => car.Price),
                    NewestModelYear = visibleCars.Count == 0 ? 0 : visibleCars.Max(car => car.Year)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể tải dữ liệu cho trang chủ showroom.");
                ViewData["LoadError"] = "Chưa thể tải dữ liệu showroom. Vui lòng kiểm tra việc khởi tạo cơ sở dữ liệu.";
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
