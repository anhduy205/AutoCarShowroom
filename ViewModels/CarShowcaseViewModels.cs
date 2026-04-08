using System.Globalization;
using AutoCarShowroom.Models;

namespace AutoCarShowroom.ViewModels
{
    public class CarCatalogIndexViewModel
    {
        public IReadOnlyList<Car> Cars { get; init; } = [];

        public IReadOnlyList<CarLineCardViewModel> Lines { get; init; } = [];

        public int TotalLines { get; init; }

        public int TotalVariants { get; init; }
    }

    public class CarLineCardViewModel
    {
        public string Brand { get; init; } = string.Empty;

        public string ModelName { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string BodyType { get; init; } = string.Empty;

        public string Image { get; init; } = string.Empty;

        public string Highlight { get; init; } = string.Empty;

        public string AccentLine { get; init; } = string.Empty;

        public decimal PriceFrom { get; init; }

        public decimal PriceTo { get; init; }

        public int VariantCount { get; init; }

        public int NewestYear { get; init; }

        public int RepresentativeCarId { get; init; }
    }

    public class CarLineViewModel
    {
        public string Brand { get; init; } = string.Empty;

        public string ModelName { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string BodyType { get; init; } = string.Empty;

        public string Highlight { get; init; } = string.Empty;

        public string Intro { get; init; } = string.Empty;

        public decimal PriceFrom { get; init; }

        public decimal PriceTo { get; init; }

        public int VariantCount { get; init; }

        public CarVariantShowcaseViewModel SelectedVariant { get; init; } = new();

        public IReadOnlyList<CarVariantShowcaseViewModel> Variants { get; init; } = [];

        public CarTechnicalSheetViewModel TechnicalSheet { get; init; } = new();

        public IReadOnlyList<CarGalleryTabViewModel> GalleryTabs { get; init; } = [];

        public IReadOnlyList<CarFeatureSectionViewModel> FeatureSections { get; init; } = [];

        public IReadOnlyList<CarLineCardViewModel> RelatedLines { get; init; } = [];

        public int? PreviousVariantId { get; init; }

        public int? NextVariantId { get; init; }
    }

    public class CarVariantShowcaseViewModel
    {
        public int CarId { get; init; }

        public string VariantName { get; init; } = string.Empty;

        public string FullName { get; init; } = string.Empty;

        public string Image { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public string Color { get; init; } = string.Empty;

        public int Year { get; init; }

        public decimal Price { get; init; }

        public string Summary { get; init; } = string.Empty;

        public string DifferenceNote { get; init; } = string.Empty;

        public bool CanOrder { get; init; }

        public bool IsSelected { get; init; }
    }

    public class CarTechnicalSheetViewModel
    {
        public string LengthText { get; init; } = string.Empty;

        public string WheelbaseText { get; init; } = string.Empty;

        public string HeightText { get; init; } = string.Empty;

        public IReadOnlyList<CarSpecItemViewModel> Items { get; init; } = [];
    }

    public class CarSpecItemViewModel
    {
        public string Label { get; init; } = string.Empty;

        public string Value { get; init; } = string.Empty;
    }

    public class CarGalleryTabViewModel
    {
        public string Id { get; init; } = string.Empty;

        public string Label { get; init; } = string.Empty;

        public IReadOnlyList<CarGalleryTileViewModel> Tiles { get; init; } = [];
    }

    public class CarGalleryTileViewModel
    {
        public string Image { get; init; } = string.Empty;

        public string Label { get; init; } = string.Empty;

        public string Caption { get; init; } = string.Empty;

        public string CssClass { get; init; } = string.Empty;
    }

    public class CarFeatureSectionViewModel
    {
        public string Title { get; init; } = string.Empty;

        public string Summary { get; init; } = string.Empty;
    }

    public static class CarShowcaseMapper
    {
        private static readonly string[] GalleryFocusClasses =
        [
            "gallery-tile--wide",
            "gallery-tile--focus-left",
            "gallery-tile--focus-center",
            "gallery-tile--focus-right",
            "gallery-tile--focus-top",
            "gallery-tile--focus-bottom"
        ];

        public static IReadOnlyList<CarLineCardViewModel> BuildLineCards(IEnumerable<Car> cars, string sortOrder)
        {
            var lines = cars
                .GroupBy(car => new { car.Brand, car.ModelName })
                .Select(group =>
                {
                    var orderedCars = group
                        .OrderByDescending(car => car.Year)
                        .ThenByDescending(car => car.Price)
                        .ToList();
                    var representativeCar = orderedCars.First();
                    var cheapestCar = group.OrderBy(car => car.Price).First();
                    var mostExpensiveCar = group.OrderByDescending(car => car.Price).First();

                    return new CarLineCardViewModel
                    {
                        Brand = representativeCar.Brand,
                        ModelName = representativeCar.ModelName,
                        DisplayName = $"{representativeCar.Brand} {representativeCar.ModelName}",
                        BodyType = NormalizeBodyType(representativeCar.BodyType),
                        Image = representativeCar.Image,
                        Highlight = BuildLineHighlight(representativeCar),
                        AccentLine = BuildLineAccent(group.Select(car => car.CarName).ToList()),
                        PriceFrom = cheapestCar.Price,
                        PriceTo = mostExpensiveCar.Price,
                        VariantCount = group.Count(),
                        NewestYear = group.Max(car => car.Year),
                        RepresentativeCarId = representativeCar.CarID
                    };
                })
                .ToList();

            return sortOrder switch
            {
                "price_desc" => lines.OrderByDescending(line => line.PriceTo).ThenBy(line => line.DisplayName).ToList(),
                "price_asc" => lines.OrderBy(line => line.PriceFrom).ThenBy(line => line.DisplayName).ToList(),
                "year_desc" => lines.OrderByDescending(line => line.NewestYear).ThenBy(line => line.DisplayName).ToList(),
                "year_asc" => lines.OrderBy(line => line.NewestYear).ThenBy(line => line.DisplayName).ToList(),
                _ => lines.OrderBy(line => line.DisplayName).ToList()
            };
        }

        public static CarLineViewModel BuildLineView(
            IReadOnlyList<Car> lineCars,
            int? variantId,
            IReadOnlyList<CarLineCardViewModel> allLines)
        {
            var orderedCars = lineCars
                .OrderByDescending(car => car.Year)
                .ThenByDescending(car => car.Price)
                .ToList();
            var selectedCar = orderedCars.FirstOrDefault(car => car.CarID == variantId) ?? orderedCars.First();
            var selectedIndex = orderedCars.FindIndex(car => car.CarID == selectedCar.CarID);
            var selectedVariant = BuildVariantViewModel(selectedCar, orderedCars, selectedIndex, true);
            var lineCard = allLines.First(line =>
                line.Brand.Equals(selectedCar.Brand, StringComparison.OrdinalIgnoreCase) &&
                line.ModelName.Equals(selectedCar.ModelName, StringComparison.OrdinalIgnoreCase));

            var relatedLines = allLines
                .Where(line => !line.DisplayName.Equals(lineCard.DisplayName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(line => line.Brand.Equals(selectedCar.Brand, StringComparison.OrdinalIgnoreCase))
                .ThenBy(line => Math.Abs(line.PriceFrom - lineCard.PriceFrom))
                .Take(4)
                .ToList();

            return new CarLineViewModel
            {
                Brand = selectedCar.Brand,
                ModelName = selectedCar.ModelName,
                DisplayName = lineCard.DisplayName,
                BodyType = NormalizeBodyType(selectedCar.BodyType),
                Highlight = lineCard.Highlight,
                Intro = BuildLineIntro(selectedCar, lineCard),
                PriceFrom = lineCard.PriceFrom,
                PriceTo = lineCard.PriceTo,
                VariantCount = orderedCars.Count,
                SelectedVariant = selectedVariant,
                Variants = orderedCars
                    .Select((car, index) => BuildVariantViewModel(car, orderedCars, index, car.CarID == selectedCar.CarID))
                    .ToList(),
                TechnicalSheet = BuildTechnicalSheet(selectedCar, selectedIndex),
                GalleryTabs = BuildGalleryTabs(selectedCar),
                FeatureSections = BuildFeatureSections(selectedCar),
                RelatedLines = relatedLines,
                PreviousVariantId = selectedIndex > 0 ? orderedCars[selectedIndex - 1].CarID : null,
                NextVariantId = selectedIndex < orderedCars.Count - 1 ? orderedCars[selectedIndex + 1].CarID : null
            };
        }

        private static CarVariantShowcaseViewModel BuildVariantViewModel(
            Car car,
            IReadOnlyList<Car> orderedCars,
            int variantIndex,
            bool isSelected)
        {
            var baseCar = orderedCars.OrderBy(item => item.Price).First();
            var delta = car.Price - baseCar.Price;

            return new CarVariantShowcaseViewModel
            {
                CarId = car.CarID,
                VariantName = BuildVariantName(car),
                FullName = car.CarName,
                Image = car.Image,
                Status = car.Status,
                Color = car.Color,
                Year = car.Year,
                Price = car.Price,
                Summary = BuildVariantSummary(car),
                DifferenceNote = delta <= 0
                    ? "Phiên bản nền để so sánh toàn dòng."
                    : $"Cao hơn bản nền {delta.ToString("N0", CultureInfo.InvariantCulture)} VNĐ.",
                CanOrder = OrderWorkflow.CanOrder(car),
                IsSelected = isSelected
            };
        }

        private static CarTechnicalSheetViewModel BuildTechnicalSheet(Car car, int variantIndex)
        {
            var signature = GetSignature(car);
            var isElectric = IsElectric(car);
            var isHybrid = IsHybrid(car);
            var isSupercar = IsSupercar(car);

            var normalizedBodyType = NormalizeBodyType(car.BodyType);
            var lengthMm = GetLengthByBodyType(normalizedBodyType) + (signature % 55);
            var wheelbaseMm = GetWheelbaseByBodyType(normalizedBodyType) + (signature % 37);
            var heightMm = GetHeightByBodyType(normalizedBodyType) + (signature % 25);
            var powerPs = GetPowerPs(car, variantIndex, signature);
            var torqueNm = GetTorqueNm(car, powerPs, signature);
            var acceleration = GetAcceleration(car, powerPs, signature);
            var topSpeed = GetTopSpeed(car, powerPs, signature);
            var priceText = $"{car.Price.ToString("N0", CultureInfo.InvariantCulture)} VNĐ";
            var consumptionLabel = isElectric ? "Mức tiêu thụ điện" : "Mức tiêu thụ kết hợp";
            var consumptionValue = GetConsumptionText(car, powerPs, isElectric, isHybrid, signature);
            var co2Value = isElectric ? "0 g/km" : $"{Math.Max(95, 115 + (signature % 150))} g/km";

            return new CarTechnicalSheetViewModel
            {
                LengthText = $"{lengthMm.ToString("N0", CultureInfo.InvariantCulture)} mm",
                WheelbaseText = $"{wheelbaseMm.ToString("N0", CultureInfo.InvariantCulture)} mm",
                HeightText = $"{heightMm.ToString("N0", CultureInfo.InvariantCulture)} mm",
                Items =
                [
                    new CarSpecItemViewModel { Label = "Công suất", Value = $"{powerPs} PS ({Math.Round(powerPs * 0.7355m):N0} kW)" },
                    new CarSpecItemViewModel { Label = "Mô men xoắn cực đại", Value = $"{torqueNm} Nm" },
                    new CarSpecItemViewModel { Label = "Tăng tốc 0 - 100 km/giờ", Value = $"{acceleration:0.0} giây" },
                    new CarSpecItemViewModel { Label = "Tốc độ tối đa", Value = $"{topSpeed} km/giờ" },
                    new CarSpecItemViewModel { Label = consumptionLabel, Value = consumptionValue },
                    new CarSpecItemViewModel { Label = "Lượng khí thải CO2", Value = co2Value },
                    new CarSpecItemViewModel { Label = "Hộp số", Value = GetTransmissionText(car, isElectric, isSupercar) },
                    new CarSpecItemViewModel { Label = "Giá tiêu chuẩn", Value = priceText }
                ]
            };
        }

        private static IReadOnlyList<CarGalleryTabViewModel> BuildGalleryTabs(Car car)
        {
            var displayName = $"{car.Brand} {car.ModelName}";
            var lineSlug = BuildLineSlug(car);
            var interiorImage = ResolveGalleryImage(lineSlug, "interior") ?? car.Image;

            return
            [
                new CarGalleryTabViewModel
                {
                    Id = "tong-quan",
                    Label = "Toàn cảnh",
                    Tiles =
                    [
                        BuildTile(
                            car.Image,
                            "Toàn cảnh mẫu xe",
                            $"{displayName} được hiển thị ở góc nhìn tổng thể để người xem nhận diện nhanh dáng xe và tỷ lệ ngoại hình.",
                            "gallery-tile--wide")
                    ]
                },
                new CarGalleryTabViewModel
                {
                    Id = "buong-lai",
                    Label = "Buồng lái",
                    Tiles =
                    [
                        BuildTile(
                            interiorImage,
                            "Không gian buồng lái",
                            $"{displayName} được trình bày theo hướng {GetInteriorMood(car)}, tập trung vào vô-lăng, cụm điều khiển và cảm giác ngồi lái.",
                            "gallery-tile--wide")
                    ]
                }
            ];
        }

        private static IReadOnlyList<CarFeatureSectionViewModel> BuildFeatureSections(Car car)
        {
            return
            [
                new CarFeatureSectionViewModel
                {
                    Title = "Thông tin chung",
                    Summary = FirstSentence(car.Specifications)
                },
                new CarFeatureSectionViewModel
                {
                    Title = "Động cơ & khung xe",
                    Summary = FirstSentence(car.EngineAndChassis)
                },
                new CarFeatureSectionViewModel
                {
                    Title = "Ngoại thất",
                    Summary = FirstSentence(car.Exterior)
                },
                new CarFeatureSectionViewModel
                {
                    Title = "Nội thất",
                    Summary = FirstSentence(car.Interior)
                },
                new CarFeatureSectionViewModel
                {
                    Title = "Ghế",
                    Summary = FirstSentence(car.Seats)
                },
                new CarFeatureSectionViewModel
                {
                    Title = "Tiện nghi",
                    Summary = FirstSentence(car.Convenience)
                },
                new CarFeatureSectionViewModel
                {
                    Title = "An ninh / chống trộm",
                    Summary = FirstSentence(car.SecurityAndAntiTheft)
                },
                new CarFeatureSectionViewModel
                {
                    Title = "An toàn chủ động",
                    Summary = FirstSentence(car.ActiveSafety)
                },
                new CarFeatureSectionViewModel
                {
                    Title = "An toàn bị động",
                    Summary = FirstSentence(car.PassiveSafety)
                }
            ];
        }

        private static CarGalleryTileViewModel BuildTile(string image, string label, string caption, string cssClass)
        {
            return new CarGalleryTileViewModel
            {
                Image = image,
                Label = label,
                Caption = caption,
                CssClass = cssClass
            };
        }

        private static string BuildVariantName(Car car)
        {
            var prefix = $"{car.Brand} {car.ModelName}".Trim();

            if (car.CarName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var trimmed = car.CarName[prefix.Length..].Trim();
                return string.IsNullOrWhiteSpace(trimmed) ? car.ModelName : trimmed;
            }

            return car.CarName;
        }

        private static string BuildVariantSummary(Car car)
        {
            return $"{car.Color}, đời {car.Year}, cấu hình {BuildVariantName(car)}. {TrimSentence(car.Description)}";
        }

        private static string BuildLineHighlight(Car car)
        {
            return TrimSentence(car.Description);
        }

        private static string BuildLineAccent(IReadOnlyList<string> variantNames)
        {
            var displayNames = variantNames
                .Take(3)
                .Select(name => name.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? name)
                .ToList();

            return displayNames.Count == 0
                ? "Nhiều phiên bản trong cùng dòng"
                : $"Các cấu hình nổi bật: {string.Join(", ", displayNames)}";
        }

        private static string BuildLineIntro(Car car, CarLineCardViewModel lineCard)
        {
            return $"{lineCard.DisplayName} đang có {lineCard.VariantCount} phiên bản, khoảng giá từ {lineCard.PriceFrom.ToString("N0", CultureInfo.InvariantCulture)} đến {lineCard.PriceTo.ToString("N0", CultureInfo.InvariantCulture)} VNĐ. Chọn từng phiên bản để xem nhanh thông số, khác biệt cấu hình và hình ảnh trưng bày.";
        }

        private static string FirstSentence(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var parts = value
                .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToList();

            return parts.Count == 0 ? value.Trim() : $"{parts[0].Trim()}.";
        }

        private static string TrimSentence(string value)
        {
            var first = FirstSentence(value);
            return string.IsNullOrWhiteSpace(first) ? value.Trim() : first;
        }

        private static int GetSignature(Car car)
        {
            var value = $"{car.Brand}|{car.ModelName}|{car.CarName}|{car.Year}|{car.Color}";
            unchecked
            {
                var hash = 17;
                foreach (var character in value)
                {
                    hash = (hash * 31) + character;
                }

                return Math.Abs(hash);
            }
        }

        private static bool IsElectric(Car car)
        {
            return car.Brand.Equals("VinFast", StringComparison.OrdinalIgnoreCase) ||
                   car.CarName.Contains("EV", StringComparison.OrdinalIgnoreCase) ||
                   car.EngineAndChassis.Contains("dien", StringComparison.OrdinalIgnoreCase) ||
                   car.EngineAndChassis.Contains("điện", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsHybrid(Car car)
        {
            return car.CarName.Contains("HEV", StringComparison.OrdinalIgnoreCase) ||
                   car.CarName.Contains("Hybrid", StringComparison.OrdinalIgnoreCase) ||
                   car.CarName.Contains("PHEV", StringComparison.OrdinalIgnoreCase) ||
                   car.EngineAndChassis.Contains("hybrid", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSupercar(Car car)
        {
            return CarCatalogMetadata.SupercarBrands.Contains(car.Brand, StringComparer.OrdinalIgnoreCase) ||
                   car.Price >= 8_000_000_000m;
        }

        private static int GetLengthByBodyType(string bodyType) => bodyType switch
        {
            "SUV" => 4740,
            "Sedan" => 4680,
            "Hatchback" => 4040,
            "MPV" => 4960,
            "Crossover" => 4510,
            "Bán tải" => 5350,
            "Coupe" => 4550,
            "Mui trần" => 4545,
            _ => 4450
        };

        private static int GetWheelbaseByBodyType(string bodyType) => bodyType switch
        {
            "SUV" => 2765,
            "Sedan" => 2735,
            "Hatchback" => 2580,
            "MPV" => 2870,
            "Crossover" => 2680,
            "Bán tải" => 3220,
            "Coupe" => 2450,
            "Mui trần" => 2450,
            _ => 2680
        };

        private static int GetHeightByBodyType(string bodyType) => bodyType switch
        {
            "SUV" => 1710,
            "Sedan" => 1440,
            "Hatchback" => 1520,
            "MPV" => 1775,
            "Crossover" => 1645,
            "Bán tải" => 1880,
            "Coupe" => 1310,
            "Mui trần" => 1305,
            _ => 1560
        };

        private static int GetPowerPs(Car car, int variantIndex, int signature)
        {
            if (IsElectric(car))
            {
                return 135 + (variantIndex * 24) + (signature % 28);
            }

            if (IsSupercar(car))
            {
                return 520 + (variantIndex * 48) + (signature % 85);
            }

            var basePower = NormalizeBodyType(car.BodyType) switch
            {
                "SUV" => 175,
                "Sedan" => 158,
                "Hatchback" => 110,
                "MPV" => 162,
                "Crossover" => 168,
                "Bán tải" => 205,
                "Coupe" => 320,
                "Mui trần" => 330,
                _ => 150
            };

            return basePower + (variantIndex * 16) + (signature % 21);
        }

        private static int GetTorqueNm(Car car, int powerPs, int signature)
        {
            if (IsElectric(car))
            {
                return 220 + (powerPs / 2) + (signature % 35);
            }

            if (IsSupercar(car))
            {
                return 580 + (powerPs / 3) + (signature % 60);
            }

            return 180 + (powerPs * 2) + (signature % 30);
        }

        private static decimal GetAcceleration(Car car, int powerPs, int signature)
        {
            if (IsElectric(car))
            {
                return Math.Max(4.8m, 9.6m - (powerPs / 65m) - (signature % 4) * 0.1m);
            }

            if (IsSupercar(car))
            {
                return Math.Max(2.7m, 4.2m - (powerPs / 500m) - (signature % 3) * 0.1m);
            }

            var bodyWeightPenalty = NormalizeBodyType(car.BodyType) switch
            {
                "SUV" => 0.9m,
                "MPV" => 1.0m,
                "Bán tải" => 1.2m,
                "Coupe" => 0.2m,
                "Mui trần" => 0.25m,
                _ => 0.55m
            };

            return Math.Max(6.3m, 13.4m - (powerPs / 28m) + bodyWeightPenalty - (signature % 3) * 0.1m);
        }

        private static int GetTopSpeed(Car car, int powerPs, int signature)
        {
            if (IsElectric(car))
            {
                return 145 + (powerPs / 4) + (signature % 8);
            }

            if (IsSupercar(car))
            {
                return 305 + (powerPs / 8) + (signature % 12);
            }

            return 175 + (powerPs / 3) + (signature % 10);
        }

        private static string GetConsumptionText(Car car, int powerPs, bool isElectric, bool isHybrid, int signature)
        {
            if (isElectric)
            {
                var kwh = 13.4m + (powerPs / 95m) + (signature % 4) * 0.3m;
                return $"{kwh:0.0} kWh/100 km";
            }

            if (isHybrid)
            {
                var liters = 4.8m + (powerPs / 210m) + (signature % 3) * 0.2m;
                return $"{liters:0.0} lít/100 km";
            }

            if (IsSupercar(car))
            {
                var liters = 11.6m + (powerPs / 180m) + (signature % 4) * 0.4m;
                return $"{liters:0.0} lít/100 km";
            }

            var normal = 5.7m + (powerPs / 120m) + (signature % 4) * 0.2m;
            return $"{normal:0.0} lít/100 km";
        }

        private static string GetTransmissionText(Car car, bool isElectric, bool isSupercar)
        {
            if (isElectric)
            {
                return "Dẫn động điện 1 cấp";
            }

            if (isSupercar)
            {
                return "Tự động ly hợp kép hiệu năng cao";
            }

            if (car.BodyType.Equals("Bán tải", StringComparison.OrdinalIgnoreCase))
            {
                return "Tự động 6 - 10 cấp";
            }

            return "Tự động/CVT tùy phiên bản";
        }

        private static string GetInteriorMood(Car car)
        {
            return car.BodyType switch
            {
                "SUV" => "rộng và nâng tầm quan sát",
                "MPV" => "thoáng và ưu tiên hàng ghế sau",
                "Coupe" => "ôm người lái, thiên về cảm giác lái",
                "Mui trần" => "mở, thể thao và giàu cảm xúc",
                _ => "gọn, hiện đại và dễ làm quen"
            };
        }
        private static string NormalizeBodyType(string bodyType)
        {
            return bodyType switch
            {
                "Bán tải" => "Bán tải",
                "Mui trần" => "Mui trần",
                "Khác" => "Khác",
                _ => bodyType
            };
        }

        private static string BuildLineSlug(Car car)
        {
            var value = $"{car.Brand}-{car.ModelName}";
            var buffer = new List<char>(value.Length);
            var previousWasDash = false;

            foreach (var character in value.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                {
                    buffer.Add(character);
                    previousWasDash = false;
                    continue;
                }

                if (previousWasDash)
                {
                    continue;
                }

                buffer.Add('-');
                previousWasDash = true;
            }

            return new string(buffer.ToArray()).Trim('-');
        }

        private static string? ResolveGalleryImage(string slug, string category)
        {
            var rootPath = Directory.GetCurrentDirectory();
            var assetFolder = Path.Combine(rootPath, "wwwroot", "images", "catalog", category);

            foreach (var extension in new[] { ".jpg", ".jpeg", ".png", ".webp" })
            {
                var physicalPath = Path.Combine(assetFolder, $"{slug}{extension}");
                if (File.Exists(physicalPath))
                {
                    return $"/images/catalog/{category}/{slug}{extension}";
                }
            }

            return null;
        }
    }
}
