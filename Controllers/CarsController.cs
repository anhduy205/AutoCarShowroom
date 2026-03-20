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
        private const long MaxImageSizeInBytes = 5 * 1024 * 1024;

        private readonly ShowroomDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CarsController(ShowroomDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string? searchTerm, string? brand, int? year, string sortOrder = "name")
        {
            try
            {
                var availableYears = await _context.Cars
                    .AsNoTracking()
                    .Select(car => car.Year)
                    .Distinct()
                    .OrderByDescending(value => value)
                    .ToListAsync();

                PopulateFilterOptions(availableYears, brand, year, sortOrder);
                ViewData["CurrentSearch"] = searchTerm;

                var carsQuery = _context.Cars.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    carsQuery = carsQuery.Where(car => car.CarName.Contains(searchTerm));
                }

                if (!string.IsNullOrWhiteSpace(brand))
                {
                    carsQuery = carsQuery.Where(car => car.Brand == brand);
                }

                if (year.HasValue)
                {
                    carsQuery = carsQuery.Where(car => car.Year == year.Value);
                }

                carsQuery = sortOrder switch
                {
                    "price_desc" => carsQuery.OrderByDescending(car => car.Price).ThenBy(car => car.CarName),
                    "price_asc" => carsQuery.OrderBy(car => car.Price).ThenBy(car => car.CarName),
                    "year_desc" => carsQuery.OrderByDescending(car => car.Year).ThenBy(car => car.CarName),
                    "year_asc" => carsQuery.OrderBy(car => car.Year).ThenBy(car => car.CarName),
                    _ => carsQuery.OrderBy(car => car.CarName)
                };

                return View(await carsQuery.ToListAsync());
            }
            catch (Exception)
            {
                PopulateFilterOptions(Array.Empty<int>(), brand, year, sortOrder);
                ViewData["LoadError"] = "Khong the tai danh sach xe. Hay kiem tra ket noi database.";
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

            return View(car);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            PopulateBrandOptions();

            return View(new CarFormViewModel
            {
                Brand = CarCatalogMetadata.MainstreamBrands.First(),
                Year = DateTime.Now.Year
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarFormViewModel model)
        {
            ValidateImageFile(model.ImageFile, imageRequired: true);

            if (!ModelState.IsValid)
            {
                PopulateBrandOptions(model.Brand);
                return View(model);
            }

            string? savedImagePath = null;

            try
            {
                savedImagePath = await SaveImageAsync(model.ImageFile!);

                var car = new Car
                {
                    Brand = model.Brand,
                    CarName = model.CarName,
                    Price = model.Price,
                    Year = model.Year,
                    Image = savedImagePath,
                    Description = model.Description
                };

                _context.Add(car);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Them xe thanh cong.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                DeleteUploadedImage(savedImagePath);
                ModelState.AddModelError(string.Empty, "Khong the luu du lieu vao database. Vui long thu lai.");
                PopulateBrandOptions(model.Brand);
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

            PopulateBrandOptions(car.Brand);
            return View(MapToFormViewModel(car));
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

            if (!ModelState.IsValid)
            {
                model.CurrentImagePath = car.Image;
                PopulateBrandOptions(model.Brand);
                return View(model);
            }

            var previousImagePath = car.Image;
            string? newImagePath = null;

            try
            {
                car.Brand = model.Brand;
                car.CarName = model.CarName;
                car.Price = model.Price;
                car.Year = model.Year;
                car.Description = model.Description;

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

                TempData["SuccessMessage"] = "Cap nhat xe thanh cong.";
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
                ModelState.AddModelError(string.Empty, "Khong the cap nhat du lieu. Vui long thu lai.");
                PopulateBrandOptions(model.Brand);
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
                TempData["SuccessMessage"] = "Xoa xe thanh cong.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Khong the xoa xe o thoi diem hien tai.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private void PopulateFilterOptions(IEnumerable<int> years, string? selectedBrand, int? selectedYear, string sortOrder)
        {
            ViewBag.Brands = new SelectList(CarCatalogMetadata.AllBrands, selectedBrand);
            ViewBag.Years = new SelectList(years, selectedYear);
            ViewBag.SortOptions = new List<SelectListItem>
            {
                new("Ten A-Z", "name", sortOrder == "name"),
                new("Gia cao den thap", "price_desc", sortOrder == "price_desc"),
                new("Gia thap den cao", "price_asc", sortOrder == "price_asc"),
                new("Nam moi den cu", "year_desc", sortOrder == "year_desc"),
                new("Nam cu den moi", "year_asc", sortOrder == "year_asc")
            };
        }

        private void PopulateBrandOptions(string? selectedBrand = null)
        {
            ViewBag.BrandOptions = new SelectList(CarCatalogMetadata.AllBrands, selectedBrand);
        }

        private static CarFormViewModel MapToFormViewModel(Car car)
        {
            return new CarFormViewModel
            {
                CarID = car.CarID,
                Brand = car.Brand,
                CarName = car.CarName,
                Price = car.Price,
                Year = car.Year,
                Description = car.Description,
                CurrentImagePath = car.Image
            };
        }

        private void ValidateImageFile(IFormFile? imageFile, bool imageRequired)
        {
            if (imageFile == null)
            {
                if (imageRequired)
                {
                    ModelState.AddModelError(nameof(CarFormViewModel.ImageFile), "Vui long chon anh xe.");
                }

                return;
            }

            if (imageFile.Length == 0)
            {
                ModelState.AddModelError(nameof(CarFormViewModel.ImageFile), "File anh khong hop le.");
                return;
            }

            if (imageFile.Length > MaxImageSizeInBytes)
            {
                ModelState.AddModelError(nameof(CarFormViewModel.ImageFile), "Anh vuot qua gioi han 5MB.");
            }

            var fileExtension = Path.GetExtension(imageFile.FileName);

            if (!AllowedImageExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(CarFormViewModel.ImageFile), "Chi chap nhan file JPG, JPEG, PNG, WEBP hoac GIF.");
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
    }
}
