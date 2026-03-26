using AutoCarShowroom.Models;
using AutoCarShowroom.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Controllers
{
    public class CarsController : Controller
    {
        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
        private static readonly string[] VehicleBodyTypes = ["SUV", "Sedan", "Hatchback", "MPV", "Crossover", "Bán tải", "Coupe", "Mui trần", "Khác"];
        private static readonly string[] VehicleStatuses =
        [
            OrderWorkflow.CarStatusAvailable,
            OrderWorkflow.CarStatusSold,
            OrderWorkflow.CarStatusPromotion
        ];

        private static readonly PriceRangeOption[] PriceRangeOptions =
        [
            new("under_700", "Dưới 700 triệu", null, 700_000_000m),
            new("700_1000", "700 triệu - 1 tỷ", 700_000_000m, 1_000_000_000m),
            new("1000_1500", "1 tỷ - 1,5 tỷ", 1_000_000_000m, 1_500_000_000m),
            new("1500_2500", "1,5 tỷ - 2,5 tỷ", 1_500_000_000m, 2_500_000_000m),
            new("above_2500", "Trên 2,5 tỷ", 2_500_000_000m, null)
        ];

        private const long MaxImageSizeInBytes = 5 * 1024 * 1024;

        private readonly ShowroomDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CarsController(ShowroomDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(
            string? searchTerm,
            string? brand,
            string? bodyType,
            string? status,
            string? priceRange,
            int? year,
            string sortOrder = "name")
        {
            try
            {
                var visibleStatuses = GetVisibleStatusesForCurrentUser();
                var visibleCarsQuery = GetVisibleCarsQuery();

                var availableBrands = await visibleCarsQuery
                    .Select(car => car.Brand)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct()
                    .OrderBy(value => value)
                    .ToListAsync();

                var availableYears = await visibleCarsQuery
                    .Select(car => car.Year)
                    .Distinct()
                    .OrderByDescending(value => value)
                    .ToListAsync();

                PopulateFilterOptions(availableBrands, availableYears, visibleStatuses, brand, bodyType, status, priceRange, year, sortOrder);
                ViewData["CurrentSearch"] = searchTerm;

                var carsQuery = visibleCarsQuery;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var keyword = searchTerm.Trim();
                    carsQuery = carsQuery.Where(car =>
                        car.CarName.Contains(keyword) ||
                        car.Brand.Contains(keyword) ||
                        car.ModelName.Contains(keyword) ||
                        car.Color.Contains(keyword) ||
                        car.BodyType.Contains(keyword) ||
                        car.Status.Contains(keyword) ||
                        car.Description.Contains(keyword));
                }

                if (year.HasValue)
                {
                    carsQuery = carsQuery.Where(car => car.Year == year.Value);
                }

                if (!string.IsNullOrWhiteSpace(brand))
                {
                    carsQuery = carsQuery.Where(car => car.Brand == brand);
                }

                if (!string.IsNullOrWhiteSpace(bodyType))
                {
                    carsQuery = carsQuery.Where(car => car.BodyType == bodyType);
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    carsQuery = carsQuery.Where(car => car.Status == status);
                }

                carsQuery = ApplyPriceRangeFilter(carsQuery, priceRange);

                var cars = await carsQuery.ToListAsync();

                cars = sortOrder switch
                {
                    "price_desc" => cars.OrderByDescending(car => car.Price).ThenBy(car => car.CarName).ToList(),
                    "price_asc" => cars.OrderBy(car => car.Price).ThenBy(car => car.CarName).ToList(),
                    "year_desc" => cars.OrderByDescending(car => car.Year).ThenBy(car => car.CarName).ToList(),
                    "year_asc" => cars.OrderBy(car => car.Year).ThenBy(car => car.CarName).ToList(),
                    _ => cars.OrderBy(car => car.CarName).ToList()
                };

                return View(cars);
            }
            catch (Exception)
            {
                PopulateFilterOptions(Array.Empty<string>(), Array.Empty<int>(), GetVisibleStatusesForCurrentUser(), brand, bodyType, status, priceRange, year, sortOrder);
                ViewData["LoadError"] = "Không thể tải danh sách xe. Vui lòng kiểm tra kết nối cơ sở dữ liệu.";
                return View(new List<Car>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .AsNoTracking()
                .FirstOrDefaultAsync(model => model.CarID == id);

            if (car == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && !OrderWorkflow.CanOrder(car))
            {
                TempData["ErrorMessage"] = "Mẫu xe này đã bán hoặc hiện không còn hiển thị cho khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            return View(car);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var viewModel = new CarFormViewModel
            {
                Year = DateTime.Now.Year,
                Status = OrderWorkflow.CarStatusAvailable
            };

            PopulateFormOptions(viewModel.BodyType, viewModel.Status);
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarFormViewModel model)
        {
            ValidateImageFile(model.ImageFile, imageRequired: true);
            ValidateVehicleMetadata(model);

            if (!ModelState.IsValid)
            {
                PopulateFormOptions(model.BodyType, model.Status);
                return View(model);
            }

            string? savedImagePath = null;

            try
            {
                savedImagePath = await SaveImageAsync(model.ImageFile!);

                var car = new Car
                {
                    CarName = model.CarName,
                    Brand = model.Brand,
                    ModelName = model.ModelName,
                    Price = model.Price,
                    Year = model.Year,
                    Color = model.Color,
                    BodyType = model.BodyType,
                    Status = model.Status,
                    Image = savedImagePath,
                    Specifications = model.Specifications,
                    Description = model.Description,
                    EngineAndChassis = model.EngineAndChassis,
                    Exterior = model.Exterior,
                    Interior = model.Interior,
                    Seats = model.Seats,
                    Convenience = model.Convenience,
                    SecurityAndAntiTheft = model.SecurityAndAntiTheft,
                    ActiveSafety = model.ActiveSafety,
                    PassiveSafety = model.PassiveSafety
                };

                _context.Add(car);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm xe thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                DeleteUploadedImage(savedImagePath);
                ModelState.AddModelError(string.Empty, "Không thể lưu dữ liệu vào cơ sở dữ liệu. Vui lòng thử lại.");
                PopulateFormOptions(model.BodyType, model.Status);
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FindAsync(id);

            if (car == null)
            {
                return NotFound();
            }

            var viewModel = MapToFormViewModel(car);
            PopulateFormOptions(viewModel.BodyType, viewModel.Status);
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarFormViewModel model)
        {
            if (id != model.CarID)
            {
                return NotFound();
            }

            var car = await _context.Cars.FindAsync(id);

            if (car == null)
            {
                return NotFound();
            }

            ValidateImageFile(model.ImageFile, imageRequired: false);
            ValidateVehicleMetadata(model);

            if (!ModelState.IsValid)
            {
                model.CurrentImagePath = car.Image;
                PopulateFormOptions(model.BodyType, model.Status);
                return View(model);
            }

            var previousImagePath = car.Image;
            string? newImagePath = null;

            try
            {
                car.CarName = model.CarName;
                car.Brand = model.Brand;
                car.ModelName = model.ModelName;
                car.Price = model.Price;
                car.Year = model.Year;
                car.Color = model.Color;
                car.BodyType = model.BodyType;
                car.Status = model.Status;
                car.Specifications = model.Specifications;
                car.Description = model.Description;
                car.EngineAndChassis = model.EngineAndChassis;
                car.Exterior = model.Exterior;
                car.Interior = model.Interior;
                car.Seats = model.Seats;
                car.Convenience = model.Convenience;
                car.SecurityAndAntiTheft = model.SecurityAndAntiTheft;
                car.ActiveSafety = model.ActiveSafety;
                car.PassiveSafety = model.PassiveSafety;

                if (model.ImageFile != null)
                {
                    newImagePath = await SaveImageAsync(model.ImageFile);
                    car.Image = newImagePath;
                }

                await _context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(newImagePath))
                {
                    DeleteUploadedImage(previousImagePath);
                }

                TempData["SuccessMessage"] = "Cập nhật xe thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                DeleteUploadedImage(newImagePath);

                if (!await CarExistsAsync(model.CarID))
                {
                    return NotFound();
                }

                throw;
            }
            catch (DbUpdateException)
            {
                DeleteUploadedImage(newImagePath);
                model.CurrentImagePath = previousImagePath;
                ModelState.AddModelError(string.Empty, "Không thể cập nhật dữ liệu. Vui lòng thử lại.");
                PopulateFormOptions(model.BodyType, model.Status);
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .AsNoTracking()
                .FirstOrDefaultAsync(model => model.CarID == id);

            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var imagePath = car.Image;

            try
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
                DeleteUploadedImage(imagePath);
                TempData["SuccessMessage"] = "Đã xóa xe thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa xe ở thời điểm hiện tại.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private void PopulateFilterOptions(
            IEnumerable<string> brands,
            IEnumerable<int> years,
            IEnumerable<string> statuses,
            string? selectedBrand,
            string? selectedBodyType,
            string? selectedStatus,
            string? selectedPriceRange,
            int? selectedYear,
            string sortOrder)
        {
            ViewBag.Brands = new SelectList(brands, selectedBrand);
            ViewBag.BodyTypeFilters = new SelectList(VehicleBodyTypes, selectedBodyType);
            ViewBag.StatusFilters = new SelectList(statuses, selectedStatus);
            ViewBag.PriceRanges = new SelectList(
                PriceRangeOptions.Select(option => new SelectListItem(option.Label, option.Value)),
                "Value",
                "Text",
                selectedPriceRange);
            ViewBag.Years = new SelectList(years, selectedYear);
            ViewBag.SortOptions = new List<SelectListItem>
            {
                new("Tên A - Z", "name", sortOrder == "name"),
                new("Giá cao đến thấp", "price_desc", sortOrder == "price_desc"),
                new("Giá thấp đến cao", "price_asc", sortOrder == "price_asc"),
                new("Năm mới đến cũ", "year_desc", sortOrder == "year_desc"),
                new("Năm cũ đến mới", "year_asc", sortOrder == "year_asc")
            };
        }

        private static IQueryable<Car> ApplyPriceRangeFilter(IQueryable<Car> carsQuery, string? priceRange)
        {
            var selectedRange = PriceRangeOptions.FirstOrDefault(option => option.Value == priceRange);

            if (selectedRange == null)
            {
                return carsQuery;
            }

            if (selectedRange.MinPrice.HasValue)
            {
                carsQuery = carsQuery.Where(car => car.Price >= selectedRange.MinPrice.Value);
            }

            if (selectedRange.MaxPrice.HasValue)
            {
                carsQuery = carsQuery.Where(car => car.Price < selectedRange.MaxPrice.Value);
            }

            return carsQuery;
        }

        private void PopulateFormOptions(string? selectedBodyType, string? selectedStatus)
        {
            ViewBag.BodyTypes = new SelectList(VehicleBodyTypes, selectedBodyType);
            ViewBag.StatusOptions = new SelectList(VehicleStatuses, selectedStatus);
        }

        private static CarFormViewModel MapToFormViewModel(Car car)
        {
            return new CarFormViewModel
            {
                CarID = car.CarID,
                CarName = car.CarName,
                Brand = car.Brand,
                ModelName = car.ModelName,
                Price = car.Price,
                Year = car.Year,
                Color = car.Color,
                BodyType = car.BodyType,
                Status = car.Status,
                Specifications = car.Specifications,
                Description = car.Description,
                EngineAndChassis = car.EngineAndChassis,
                Exterior = car.Exterior,
                Interior = car.Interior,
                Seats = car.Seats,
                Convenience = car.Convenience,
                SecurityAndAntiTheft = car.SecurityAndAntiTheft,
                ActiveSafety = car.ActiveSafety,
                PassiveSafety = car.PassiveSafety,
                CurrentImagePath = car.Image
            };
        }

        private void ValidateVehicleMetadata(CarFormViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.BodyType) &&
                !VehicleBodyTypes.Contains(model.BodyType, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(CarFormViewModel.BodyType), "Loại xe không hợp lệ.");
            }

            if (!string.IsNullOrWhiteSpace(model.Status) &&
                !VehicleStatuses.Contains(model.Status, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(CarFormViewModel.Status), "Trạng thái xe không hợp lệ.");
            }
        }

        private void ValidateImageFile(IFormFile? imageFile, bool imageRequired)
        {
            if (imageFile == null)
            {
                if (imageRequired)
                {
                    ModelState.AddModelError(nameof(CarFormViewModel.ImageFile), "Vui lòng chọn ảnh xe.");
                }

                return;
            }

            if (imageFile.Length == 0)
            {
                ModelState.AddModelError(nameof(CarFormViewModel.ImageFile), "Tệp ảnh không hợp lệ.");
                return;
            }

            if (imageFile.Length > MaxImageSizeInBytes)
            {
                ModelState.AddModelError(nameof(CarFormViewModel.ImageFile), "Ảnh vượt quá giới hạn 5MB.");
            }

            var fileExtension = Path.GetExtension(imageFile.FileName);

            if (!AllowedImageExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(CarFormViewModel.ImageFile), "Chỉ chấp nhận tệp JPG, JPEG, PNG, WEBP hoặc GIF.");
            }
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "cars");
            Directory.CreateDirectory(uploadFolder);

            var fileExtension = Path.GetExtension(imageFile.FileName);
            var fileName = $"{Guid.NewGuid():N}{fileExtension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(fileStream);

            return $"/uploads/cars/{fileName}";
        }

        private void DeleteUploadedImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) ||
                !imagePath.StartsWith("/uploads/cars/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);

            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }

        private Task<bool> CarExistsAsync(int id)
        {
            return _context.Cars.AnyAsync(car => car.CarID == id);
        }

        private IQueryable<Car> GetVisibleCarsQuery()
        {
            var carsQuery = _context.Cars.AsNoTracking().AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                carsQuery = carsQuery.Where(car => OrderWorkflow.PurchasableCarStatuses.Contains(car.Status));
            }

            return carsQuery;
        }

        private IReadOnlyList<string> GetVisibleStatusesForCurrentUser()
        {
            return User.IsInRole("Admin")
                ? VehicleStatuses
                : OrderWorkflow.PurchasableCarStatuses;
        }

        private sealed record PriceRangeOption(string Value, string Label, decimal? MinPrice, decimal? MaxPrice);
    }
}
