using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AutoCarShowroom.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCarShowroom.Services
{
    public class ShowroomChatbotService
    {
        private static readonly IReadOnlyDictionary<string, string[]> PurposeBodyTypeMap = new Dictionary<string, string[]>
        {
            ["gia đình"] = ["SUV", "MPV", "Crossover"],
            ["đi phố"] = ["Sedan", "Hatchback", "Crossover"],
            ["dịch vụ"] = ["Sedan", "MPV"],
            ["đường dài"] = ["SUV", "Crossover", "Bán tải"],
            ["doanh nhân"] = ["Sedan", "SUV"],
            ["offroad"] = ["SUV", "Bán tải"]
        };

        private readonly ShowroomDbContext _context;

        public ShowroomChatbotService(ShowroomDbContext context)
        {
            _context = context;
        }

        public async Task<ChatbotReply> AskAsync(string? message)
        {
            var normalizedMessage = Normalize(message);

            if (string.IsNullOrWhiteSpace(normalizedMessage))
            {
                return new ChatbotReply
                {
                    Message = "Mình có thể gợi ý xe theo ngân sách, loại xe hoặc mục đích sử dụng. Bạn có thể hỏi như: “SUV dưới 1 tỷ”, “xe gia đình”, hoặc “so sánh Camry và C300”."
                };
            }

            var cars = await _context.Cars
                .AsNoTracking()
                .Where(car => OrderWorkflow.PurchasableCarStatuses.Contains(car.Status))
                .ToListAsync();

            if (cars.Count == 0)
            {
                return new ChatbotReply
                {
                    Message = "Hiện showroom chưa có mẫu xe đang mở bán để tư vấn. Bạn thử cập nhật dữ liệu xe rồi nhắn lại nhé."
                };
            }

            if (IsComparisonQuery(normalizedMessage))
            {
                var comparisonReply = BuildComparisonReply(normalizedMessage, cars);
                if (comparisonReply != null)
                {
                    return comparisonReply;
                }
            }

            return BuildRecommendationReply(normalizedMessage, cars);
        }

        private static bool IsComparisonQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("so sanh", StringComparison.OrdinalIgnoreCase) ||
                   normalizedMessage.Contains("compare", StringComparison.OrdinalIgnoreCase);
        }

        private ChatbotReply? BuildComparisonReply(string normalizedMessage, IReadOnlyList<Car> cars)
        {
            var matchedCars = cars
                .Where(car =>
                    normalizedMessage.Contains(Normalize(car.ModelName), StringComparison.OrdinalIgnoreCase) ||
                    normalizedMessage.Contains(Normalize(car.CarName), StringComparison.OrdinalIgnoreCase))
                .Take(2)
                .ToList();

            if (matchedCars.Count < 2)
            {
                return null;
            }

            var firstCar = matchedCars[0];
            var secondCar = matchedCars[1];

            var builder = new StringBuilder();
            builder.Append($"Mình đã so sánh nhanh {firstCar.CarName} với {secondCar.CarName}. ");
            builder.Append(firstCar.Price <= secondCar.Price
                ? $"{firstCar.ModelName} đang mềm giá hơn, "
                : $"{secondCar.ModelName} đang mềm giá hơn, ");
            builder.Append(firstCar.Year >= secondCar.Year
                ? $"{firstCar.ModelName} có đời xe mới hơn hoặc tương đương. "
                : $"{secondCar.ModelName} có đời xe mới hơn hoặc tương đương. ");
            builder.Append("Bạn có thể mở chi tiết 2 mẫu dưới đây để xem kỹ ngoại hình, giá và thông số.");

            return new ChatbotReply
            {
                Message = builder.ToString(),
                Suggestions =
                [
                    BuildSuggestion(firstCar, $"Hợp nếu bạn ưu tiên {firstCar.BodyType.ToLowerInvariant()} với mức giá {firstCar.Price:N0} VNĐ."),
                    BuildSuggestion(secondCar, $"Phù hợp để đối chiếu thêm trong cùng nhóm {secondCar.BodyType.ToLowerInvariant()} ở mức {secondCar.Price:N0} VNĐ.")
                ]
            };
        }

        private ChatbotReply BuildRecommendationReply(string normalizedMessage, IReadOnlyList<Car> cars)
        {
            var distinctBrands = cars.Select(car => car.Brand).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var detectedBrands = distinctBrands
                .Where(brand => normalizedMessage.Contains(Normalize(brand), StringComparison.OrdinalIgnoreCase))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var detectedBodyTypes = cars
                .Select(car => car.BodyType)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(bodyType => normalizedMessage.Contains(Normalize(bodyType), StringComparison.OrdinalIgnoreCase))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var detectedPurpose = PurposeBodyTypeMap.Keys
                .FirstOrDefault(keyword => normalizedMessage.Contains(Normalize(keyword), StringComparison.OrdinalIgnoreCase));

            var preferredBodyTypes = detectedPurpose != null
                ? PurposeBodyTypeMap[detectedPurpose]
                : Array.Empty<string>();

            var budget = ExtractBudget(normalizedMessage);

            var rankedCars = cars
                .Select(car =>
                {
                    var score = 0;
                    var reasons = new List<string>();

                    if (detectedBrands.Contains(car.Brand))
                    {
                        score += 90;
                        reasons.Add($"đúng hãng {car.Brand}");
                    }

                    if (detectedBodyTypes.Contains(car.BodyType))
                    {
                        score += 75;
                        reasons.Add($"thuộc nhóm {car.BodyType}");
                    }

                    if (preferredBodyTypes.Contains(car.BodyType, StringComparer.OrdinalIgnoreCase))
                    {
                        score += 60;
                        reasons.Add($"hợp nhu cầu {detectedPurpose}");
                    }

                    if (budget.HasValue)
                    {
                        if (car.Price <= budget.Value)
                        {
                            score += 55;
                            reasons.Add($"nằm trong ngân sách khoảng {budget.Value:N0} VNĐ");
                        }
                        else
                        {
                            var overBudgetRatio = (double)((car.Price - budget.Value) / Math.Max(budget.Value, 1));
                            score -= (int)Math.Round(overBudgetRatio * 40);
                        }
                    }

                    if (string.Equals(car.Status, OrderWorkflow.CarStatusPromotion, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 20;
                        reasons.Add("đang có trạng thái khuyến mãi");
                    }

                    score += car.Year;

                    return new
                    {
                        Car = car,
                        Score = score,
                        Reason = reasons.Count > 0
                            ? $"Phù hợp vì {string.Join(", ", reasons)}."
                            : $"Đây là gợi ý an toàn nhờ đời xe {car.Year} và mức giá {car.Price:N0} VNĐ."
                    };
                })
                .OrderByDescending(item => item.Score)
                .ThenByDescending(item => item.Car.Year)
                .ThenBy(item => item.Car.Price)
                .Take(3)
                .ToList();

            var introParts = new List<string>();

            if (budget.HasValue)
            {
                introParts.Add($"ngân sách khoảng {budget.Value:N0} VNĐ");
            }

            if (detectedBrands.Count > 0)
            {
                introParts.Add($"hãng {string.Join(", ", detectedBrands)}");
            }

            if (detectedBodyTypes.Count > 0)
            {
                introParts.Add($"loại xe {string.Join(", ", detectedBodyTypes)}");
            }

            if (!string.IsNullOrWhiteSpace(detectedPurpose))
            {
                introParts.Add($"nhu cầu {detectedPurpose}");
            }

            var intro = introParts.Count > 0
                ? $"Mình đang ưu tiên các xe phù hợp với {string.Join(", ", introParts)}. "
                : "Mình đang lấy các mẫu dễ bắt đầu nhất theo dữ liệu showroom hiện tại. ";

            return new ChatbotReply
            {
                Message = intro + "Bạn có thể bắt đầu từ các gợi ý dưới đây rồi xem chi tiết từng mẫu để so tiếp.",
                Suggestions = rankedCars
                    .Select(item => BuildSuggestion(item.Car, item.Reason))
                    .ToList()
            };
        }

        private static ChatbotSuggestion BuildSuggestion(Car car, string reason)
        {
            return new ChatbotSuggestion
            {
                CarId = car.CarID,
                CarName = car.CarName,
                Brand = car.Brand,
                ModelName = car.ModelName,
                BodyType = car.BodyType,
                Image = car.Image,
                Price = car.Price,
                Reason = reason
            };
        }

        private static decimal? ExtractBudget(string normalizedMessage)
        {
            var billionMatch = Regex.Match(normalizedMessage, @"(\d+(?:[.,]\d+)?)\s*(ty|ti)");
            if (billionMatch.Success && TryParseDecimal(billionMatch.Groups[1].Value, out var billionValue))
            {
                return billionValue * 1_000_000_000m;
            }

            var millionMatch = Regex.Match(normalizedMessage, @"(\d+(?:[.,]\d+)?)\s*(trieu|tr)");
            if (millionMatch.Success && TryParseDecimal(millionMatch.Groups[1].Value, out var millionValue))
            {
                return millionValue * 1_000_000m;
            }

            var directAmountMatch = Regex.Match(normalizedMessage, @"\b\d{9,12}\b");
            if (directAmountMatch.Success && decimal.TryParse(directAmountMatch.Value, out var directValue))
            {
                return directValue;
            }

            return null;
        }

        private static bool TryParseDecimal(string rawValue, out decimal value)
        {
            var normalizedValue = rawValue.Replace(",", ".", StringComparison.Ordinal);
            return decimal.TryParse(
                normalizedValue,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out value);
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder
                .ToString()
                .Replace('đ', 'd')
                .Replace('Đ', 'D')
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant();
        }
    }
}
