using AutoCarShowroom.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Services.Chatbot
{
    public sealed class ChatbotInventoryTools
    {
        private static readonly IReadOnlyDictionary<string, string[]> PurposeBodyTypeMap = new Dictionary<string, string[]>
        {
            ["gia dinh"] = ["SUV", "MPV", "Crossover", "Sedan"],
            ["ca nhan"] = ["Sedan", "Hatchback", "Crossover"],
            ["di pho"] = ["Sedan", "Hatchback", "Crossover"],
            ["duong dai"] = ["SUV", "Crossover", "MPV", "Bán tải"],
            ["dich vu"] = ["Sedan", "MPV"],
            ["doanh nhan"] = ["Sedan", "SUV"],
            ["mua lan dau"] = ["Sedan", "Hatchback", "Crossover"],
            ["offroad"] = ["SUV", "Bán tải"]
        };

        private readonly ShowroomDbContext _context;

        public ChatbotInventoryTools(ShowroomDbContext context)
        {
            _context = context;
        }

        public Task<Car?> GetCarByIdAsync(int carId)
        {
            return QueryVisibleCarsLite()
                .FirstOrDefaultAsync(car => car.CarID == carId);
        }

        public async Task<IReadOnlyList<Car>> MatchCarsByMessageAsync(string? message, int? preferredCarId = null, int take = 3)
        {
            var cars = await QueryVisibleCarsLite().ToListAsync();
            var normalizedMessage = ChatbotTextParser.Normalize(message);

            var matches = cars
                .Select(car =>
                {
                    var normalizedCarName = ChatbotTextParser.Normalize(car.CarName);
                    var normalizedModelName = ChatbotTextParser.Normalize(car.ModelName);
                    var normalizedBrandModel = ChatbotTextParser.Normalize($"{car.Brand} {car.ModelName}");
                    var normalizedBrandCarName = ChatbotTextParser.Normalize($"{car.Brand} {car.CarName}");

                    var score = 0;
                    if (normalizedMessage.Contains(normalizedCarName, StringComparison.Ordinal))
                    {
                        score += 300;
                    }

                    if (normalizedMessage.Contains(normalizedBrandCarName, StringComparison.Ordinal))
                    {
                        score += 240;
                    }

                    if (normalizedMessage.Contains(normalizedBrandModel, StringComparison.Ordinal))
                    {
                        score += 180;
                    }

                    if (normalizedMessage.Contains(normalizedModelName, StringComparison.Ordinal))
                    {
                        score += 80;
                    }

                    if (preferredCarId.HasValue && car.CarID == preferredCarId.Value)
                    {
                        score += 40;
                    }

                    return new
                    {
                        Car = car,
                        Score = score
                    };
                })
                .Where(match => match.Score > 0)
                .OrderByDescending(match => match.Score)
                .ThenByDescending(match => match.Car.Year)
                .ThenBy(match => match.Car.Price)
                .Select(match => match.Car)
                .Take(take)
                .ToList();

            if (matches.Count == 0 && preferredCarId.HasValue)
            {
                var preferredCar = cars.FirstOrDefault(car => car.CarID == preferredCarId.Value);
                if (preferredCar != null && ChatbotTextParser.ContainsAny(normalizedMessage, "xe nay", "mau nay", "xe hien tai"))
                {
                    matches.Add(preferredCar);
                }
            }

            return matches;
        }

        public async Task<IReadOnlyList<ChatbotCarMatch>> SearchCarsAsync(ChatbotSearchCriteria criteria)
        {
            var cars = await QueryVisibleCarsLite().ToListAsync();
            var preferredBodyTypes = GetPreferredBodyTypes(criteria.Purpose);
            var normalizedPreferredBodyTypes = preferredBodyTypes
                .Select(ChatbotTextParser.Normalize)
                .ToHashSet(StringComparer.Ordinal);
            var keywordTokens = ChatbotTextParser.TokenizeSearchKeywords(criteria.Keyword);
            var normalizedRequestedBodyType = ChatbotTextParser.Normalize(criteria.BodyType);
            var filteredCars = cars
                .Where(car => !criteria.ExcludedCarId.HasValue || car.CarID != criteria.ExcludedCarId.Value)
                .Where(car => string.IsNullOrWhiteSpace(criteria.Brand)
                    || string.Equals(criteria.Brand, car.Brand, StringComparison.OrdinalIgnoreCase))
                .Where(car => string.IsNullOrWhiteSpace(normalizedRequestedBodyType)
                    || string.Equals(normalizedRequestedBodyType, ChatbotTextParser.Normalize(car.BodyType), StringComparison.Ordinal))
                .Where(car => !criteria.SeatCount.HasValue
                    || (!string.IsNullOrWhiteSpace(car.Seats)
                        && car.Seats.Contains(criteria.SeatCount.Value.ToString(), StringComparison.OrdinalIgnoreCase)))
                .Where(car => !criteria.MinBudget.HasValue || car.Price >= criteria.MinBudget.Value)
                .Where(car => !criteria.MaxBudget.HasValue || car.Price <= criteria.MaxBudget.Value)
                .ToList();

            if (filteredCars.Count == 0)
            {
                return [];
            }

            var rankedCars = filteredCars
                .Select(car =>
                {
                    var score = 0;
                    var matchedReasons = new List<string>();
                    var considerations = new List<string>();
                    var normalizedCarBodyType = ChatbotTextParser.Normalize(car.BodyType);

                    if (!string.IsNullOrWhiteSpace(criteria.Brand))
                    {
                        score += 90;
                        matchedReasons.Add($"đúng hãng {car.Brand}");
                    }

                    if (!string.IsNullOrWhiteSpace(normalizedRequestedBodyType))
                    {
                        score += 80;
                        matchedReasons.Add($"thuộc nhóm {car.BodyType}");
                    }

                    if (criteria.SeatCount.HasValue)
                    {
                        score += 75;
                        matchedReasons.Add($"có phương án {criteria.SeatCount.Value} chỗ");
                    }

                    if (normalizedPreferredBodyTypes.Contains(normalizedCarBodyType))
                    {
                        score += 60;
                        matchedReasons.Add($"phù hợp nhu cầu {criteria.Purpose}");
                    }

                    if (criteria.MinBudget.HasValue)
                    {
                        score += 70;
                        matchedReasons.Add($"nằm trong mức giá từ {criteria.MinBudget.Value:N0} VNĐ");
                    }

                    if (criteria.MaxBudget.HasValue)
                    {
                        score += 70;
                        matchedReasons.Add($"nằm trong ngân sách đến khoảng {criteria.MaxBudget.Value:N0} VNĐ");
                    }

                    if (keywordTokens.Count > 0)
                    {
                        var searchableText = ChatbotTextParser.Normalize(
                            string.Join(' ',
                                car.CarName,
                                car.Brand,
                                car.ModelName,
                                car.Description,
                                car.Specifications,
                                car.EngineAndChassis,
                                car.Seats,
                                car.Convenience,
                                car.ActiveSafety,
                                car.PassiveSafety));

                        var matchedTokenCount = keywordTokens.Count(token =>
                            searchableText.Contains(token, StringComparison.Ordinal));

                        if (matchedTokenCount > 0)
                        {
                            score += matchedTokenCount * 15;
                            matchedReasons.Add("khớp thêm với nhu cầu mô tả");
                        }
                    }

                    if (string.Equals(car.Status, OrderWorkflow.CarStatusPromotion, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 15;
                        matchedReasons.Add("đang có trạng thái khuyến mãi");
                    }

                    score += car.Year;

                    if (matchedReasons.Count == 0)
                    {
                        matchedReasons.Add($"đời xe {car.Year} và đang hiển thị trong showroom");
                    }

                    if (considerations.Count == 0)
                    {
                        considerations.Add(GetDefaultConsideration(car));
                    }

                    return new ChatbotCarMatch
                    {
                        Car = car,
                        Score = score,
                        MatchedReasons = matchedReasons,
                        Considerations = considerations
                    };
                })
                .OrderByDescending(match => match.Score)
                .ThenByDescending(match => match.Car.Year)
                .ThenBy(match => match.Car.Price)
                .Take(Math.Clamp(criteria.MaxResults, 1, 5))
                .ToList();

            return rankedCars;
        }

        public async Task<IReadOnlyList<Car>> GetPromotionCarsAsync(int take = 4)
        {
            return await QueryVisibleCarsLite()
                .Where(car => car.Status == OrderWorkflow.CarStatusPromotion)
                .OrderByDescending(car => car.Year)
                .ThenByDescending(car => car.Price)
                .Take(take)
                .ToListAsync();
        }

        public async Task<ChatbotInventoryOverview> GetInventoryOverviewAsync(int featuredTake = 4)
        {
            var visibleCars = QueryVisibleCars();
            var totalVisibleCars = await visibleCars.CountAsync();

            if (totalVisibleCars == 0)
            {
                return new ChatbotInventoryOverview();
            }

            var bodyTypeGroups = await visibleCars
                .GroupBy(car => car.BodyType)
                .Select(group => new
                {
                    Key = group.Key,
                    Count = group.Count()
                })
                .ToListAsync();

            var brandGroups = await visibleCars
                .GroupBy(car => car.Brand)
                .Select(group => new
                {
                    Key = group.Key,
                    Count = group.Count()
                })
                .ToListAsync();

            var featuredCars = await QueryVisibleCarsLite()
                .OrderByDescending(car => car.Status == OrderWorkflow.CarStatusPromotion)
                .ThenByDescending(car => car.Year)
                .ThenBy(car => car.Price)
                .Take(Math.Clamp(featuredTake, 1, 6))
                .ToListAsync();

            var bodyTypeSummaries = bodyTypeGroups
                .OrderByDescending(group => group.Count)
                .ThenBy(group => group.Key)
                .Select(group => $"{group.Key}: {group.Count} xe")
                .ToList();

            var brandSummaries = brandGroups
                .OrderByDescending(group => group.Count)
                .ThenBy(group => group.Key)
                .Take(6)
                .Select(group => $"{group.Key}: {group.Count} xe")
                .ToList();

            return new ChatbotInventoryOverview
            {
                TotalVisibleCars = totalVisibleCars,
                MinPrice = featuredCars.Count > 0
                    ? await visibleCars.MinAsync(car => car.Price)
                    : 0m,
                MaxPrice = featuredCars.Count > 0
                    ? await visibleCars.MaxAsync(car => car.Price)
                    : 0m,
                BodyTypeSummaries = bodyTypeSummaries,
                BrandSummaries = brandSummaries,
                FeaturedCars = featuredCars
            };
        }

        private IQueryable<Car> QueryVisibleCars()
        {
            return _context.Cars
                .AsNoTracking()
                .Where(car => OrderWorkflow.PurchasableCarStatuses.Contains(car.Status));
        }

        private IQueryable<Car> QueryVisibleCarsLite()
        {
            return QueryVisibleCars()
                .Select(car => new Car
                {
                    CarID = car.CarID,
                    CarName = car.CarName,
                    Brand = car.Brand,
                    ModelName = car.ModelName,
                    BodyType = car.BodyType,
                    Image = car.Image,
                    Price = car.Price,
                    Status = car.Status,
                    Year = car.Year,
                    Description = car.Description,
                    Specifications = car.Specifications,
                    EngineAndChassis = car.EngineAndChassis,
                    Seats = car.Seats,
                    Convenience = car.Convenience,
                    ActiveSafety = car.ActiveSafety,
                    PassiveSafety = car.PassiveSafety
                });
        }

        private static IReadOnlyList<string> GetPreferredBodyTypes(string? purpose)
        {
            if (string.IsNullOrWhiteSpace(purpose))
            {
                return [];
            }

            return PurposeBodyTypeMap.TryGetValue(ChatbotTextParser.Normalize(purpose), out var bodyTypes)
                ? bodyTypes
                : [];
        }

        private static string GetDefaultConsideration(Car car)
        {
            return car.BodyType switch
            {
                "SUV" => "gầm cao đi tỉnh và leo lề thuận tiện hơn, nhưng chi phí sử dụng có thể nhỉnh hơn sedan",
                "MPV" => "thiên về không gian và tính thực dụng hơn cảm giác lái",
                "Sedan" => "đi phố và đường trường ổn, nhưng độ linh hoạt đường xấu sẽ không bằng SUV",
                "Hatchback" => "gọn và dễ xoay xở, nhưng khoang hành lý sẽ vừa phải hơn",
                "Bán tải" => "hợp đường xấu và chở đồ, nhưng kích thước lớn hơn khi đi phố",
                _ => $"cần xem kỹ thêm phần mô tả và thông số chi tiết của {car.CarName} trước khi chốt"
            };
        }
    }
}
