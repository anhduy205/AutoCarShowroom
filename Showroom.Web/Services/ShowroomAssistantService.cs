using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Showroom.Web.Configuration;
using Showroom.Web.Models;

namespace Showroom.Web.Services;

public class ShowroomAssistantService : IShowroomAssistantService
{
    private static readonly IReadOnlyList<string> DefaultSuggestions =
    [
        "Showroom hien co bao nhieu xe?",
        "Gia xe Hyundai bao nhieu?",
        "Xe nao dang ban chay?"
    ];

    private readonly IShowroomDataService _showroomDataService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GeminiOptions _geminiOptions;
    private readonly ILogger<ShowroomAssistantService> _logger;

    public ShowroomAssistantService(
        IShowroomDataService showroomDataService,
        IHttpClientFactory httpClientFactory,
        IOptions<GeminiOptions> geminiOptions,
        ILogger<ShowroomAssistantService> logger)
    {
        _showroomDataService = showroomDataService;
        _httpClientFactory = httpClientFactory;
        _geminiOptions = geminiOptions.Value;
        _logger = logger;
    }

    public async Task<AiChatReplyViewModel> GetReplyAsync(string? message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return new AiChatReplyViewModel
            {
                Reply = "Toi la tro ly AI cua showroom. Ban co the hoi ve tong so xe, gia xe theo hang, ton kho, hoac nhung mau xe ban chay.",
                Suggestions = DefaultSuggestions
            };
        }

        var knowledge = await _showroomDataService.GetShowroomKnowledgeAsync(cancellationToken);
        var dashboard = knowledge.Dashboard;
        var normalizedMessage = Normalize(message);

        if (!dashboard.IsDatabaseConnected)
        {
            return new AiChatReplyViewModel
            {
                Reply = "Toi chua ket noi duoc du lieu showroom nen chua the tra loi chinh xac theo ton kho. Ban van co the thu lai sau khi he thong ket noi SQL Server on dinh.",
                Suggestions = DefaultSuggestions
            };
        }

        if (HasGeminiConfiguration())
        {
            var aiReply = await TryGetGeminiReplyAsync(message, knowledge, cancellationToken);
            if (!string.IsNullOrWhiteSpace(aiReply))
            {
                return new AiChatReplyViewModel
                {
                    Reply = aiReply,
                    Suggestions = DefaultSuggestions
                };
            }
        }

        if (TryAnswerPriceQuestion(normalizedMessage, knowledge.Cars, out var priceReply))
        {
            return new AiChatReplyViewModel
            {
                Reply = priceReply,
                Suggestions =
                [
                    "Xe Hyundai nao dang con hang?",
                    "Showroom hien co bao nhieu xe?",
                    "Xe nao dang ban chay?"
                ]
            };
        }

        if (IsGreeting(normalizedMessage))
        {
            return new AiChatReplyViewModel
            {
                Reply = $"Xin chao. Hien showroom dang quan ly {dashboard.TotalCars} xe thuoc {dashboard.BrandInventory.Count} hang. Ban muon xem ton kho, gia xe hay xe ban chay?",
                Suggestions = DefaultSuggestions
            };
        }

        if (AsksForTotalCars(normalizedMessage))
        {
            return new AiChatReplyViewModel
            {
                Reply = $"Showroom hien co tong cong {dashboard.TotalCars} xe dang duoc quan ly trong he thong.",
                Suggestions =
                [
                    "Gia xe Hyundai bao nhieu?",
                    "Hang nao dang co nhieu xe nhat?",
                    "Xe nao dang ban chay?"
                ]
            };
        }

        if (AsksForBrands(normalizedMessage))
        {
            var topBrands = dashboard.BrandInventory
                .Where(item => item.CarCount > 0)
                .Take(4)
                .Select(item => $"{item.BrandName} ({item.CarCount} xe)")
                .ToList();

            if (topBrands.Count == 0)
            {
                return new AiChatReplyViewModel
                {
                    Reply = "Hien chua co du lieu hang xe trong showroom.",
                    Suggestions = DefaultSuggestions
                };
            }

            var leadingBrand = dashboard.BrandInventory
                .OrderByDescending(item => item.CarCount)
                .ThenBy(item => item.BrandName)
                .First();

            return new AiChatReplyViewModel
            {
                Reply = $"Hang dang co nhieu xe nhat la {leadingBrand.BrandName} voi {leadingBrand.CarCount} xe. Mot so hang dang co san: {string.Join(", ", topBrands)}.",
                Suggestions =
                [
                    "Gia xe Hyundai bao nhieu?",
                    "Xe nao dang ban chay?",
                    "Xe Toyota nao dang con hang?"
                ]
            };
        }

        if (AsksForBestSellers(normalizedMessage))
        {
            if (dashboard.BestSellingCars.Count == 0)
            {
                return new AiChatReplyViewModel
                {
                    Reply = "He thong chua co du lieu xe ban chay tu don hang hoan tat.",
                    Suggestions =
                    [
                        "Showroom hien co bao nhieu xe?",
                        "Gia xe Hyundai bao nhieu?"
                    ]
                };
            }

            var summary = string.Join(", ",
                dashboard.BestSellingCars.Take(3).Select(item => $"{item.CarName} - {item.BrandName} ({item.SoldQuantity} luot)"));

            return new AiChatReplyViewModel
            {
                Reply = $"Nhung mau xe noi bat nhat hien tai la: {summary}. Ban muon xem them gia xe hay ton kho theo hang khong?",
                Suggestions =
                [
                    "Gia xe Hyundai bao nhieu?",
                    "Xe Ford nao dang con hang?"
                ]
            };
        }

        return new AiChatReplyViewModel
        {
            Reply = "Toi co the giup ban ve tong so xe, gia xe theo hang, ton kho va xe ban chay. Ban co the hoi vi du nhu gia xe Hyundai, xe Toyota con hang, hoac tong so xe trong showroom.",
            Suggestions = DefaultSuggestions
        };
    }

    private static bool IsGreeting(string message) =>
        message.Contains("xin chao") ||
        message.Contains("chao") ||
        message.Contains("hello") ||
        message == "hi";

    private static bool AsksForTotalCars(string message) =>
        message.Contains("bao nhieu xe") ||
        message.Contains("tong so xe") ||
        message.Contains("tong cong bao nhieu") ||
        (message.Contains("so luong") && message.Contains("xe"));

    private static bool AsksForBrands(string message) =>
        message.Contains("hang nao") ||
        message.Contains("thuong hieu") ||
        message.Contains("hang xe") ||
        message.Contains("nhieu xe nhat");

    private static bool AsksForBestSellers(string message) =>
        message.Contains("ban chay") ||
        message.Contains("noi bat") ||
        message.Contains("top xe");

    private static bool TryAnswerPriceQuestion(string normalizedMessage, IReadOnlyList<CarListItemViewModel> cars, out string reply)
    {
        reply = string.Empty;

        if (!normalizedMessage.Contains("gia") && !normalizedMessage.Contains("bao nhieu tien"))
        {
            return false;
        }

        var matchedCars = cars
            .Where(car =>
                normalizedMessage.Contains(Normalize(car.BrandName)) ||
                normalizedMessage.Contains(Normalize(car.Name)))
            .ToList();

        if (matchedCars.Count == 0)
        {
            return false;
        }

        reply = string.Join(" ", matchedCars.Select(car =>
            $"{car.Name} thuoc hang {car.BrandName} dang co gia {car.Price:N0} VND va ton kho {car.StockQuantity} xe."));

        return true;
    }

    private bool HasGeminiConfiguration() =>
        !string.IsNullOrWhiteSpace(_geminiOptions.ApiKey) &&
        !string.IsNullOrWhiteSpace(_geminiOptions.Model) &&
        !string.IsNullOrWhiteSpace(_geminiOptions.Endpoint);

    private async Task<string?> TryGetGeminiReplyAsync(
        string userMessage,
        ShowroomKnowledgeViewModel knowledge,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestUri = $"{_geminiOptions.Endpoint.TrimEnd('/')}/v1beta/models/{_geminiOptions.Model}:generateContent";
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("x-goog-api-key", _geminiOptions.ApiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = JsonContent.Create(new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new
                            {
                                text = BuildPrompt(userMessage, knowledge)
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.6,
                    topP = 0.9,
                    maxOutputTokens = 260
                }
            });

            var client = _httpClientFactory.CreateClient();
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gemini API returned status code {StatusCode}.", response.StatusCode);
                return null;
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                return null;
            }

            var firstCandidate = candidates[0];
            if (!firstCandidate.TryGetProperty("content", out var content) ||
                !content.TryGetProperty("parts", out var parts) ||
                parts.GetArrayLength() == 0)
            {
                return null;
            }

            var responseText = parts[0].GetProperty("text").GetString();
            return string.IsNullOrWhiteSpace(responseText) ? null : responseText.Trim();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Could not get a reply from Gemini API.");
            return null;
        }
    }

    private static string BuildPrompt(string userMessage, ShowroomKnowledgeViewModel knowledge)
    {
        var dashboard = knowledge.Dashboard;

        var brands = dashboard.BrandInventory.Count == 0
            ? "Chua co du lieu hang xe."
            : string.Join(", ", dashboard.BrandInventory.Select(item => $"{item.BrandName}: {item.CarCount} xe"));

        var bestSellingCars = dashboard.BestSellingCars.Count == 0
            ? "Chua co du lieu xe ban chay."
            : string.Join(", ", dashboard.BestSellingCars.Select(item => $"{item.CarName} - {item.BrandName}: {item.SoldQuantity} luot"));

        var carCatalog = knowledge.Cars.Count == 0
            ? "Chua co du lieu chi tiet xe."
            : string.Join("; ", knowledge.Cars.Select(car =>
                $"{car.Name} | Hang: {car.BrandName} | Gia: {car.Price:N0} VND | Ton kho: {car.StockQuantity}"));

        return $"""
            Ban la tro ly AI cho website showroom o to. Hay tra loi bang tieng Viet, ngan gon, tu nhien, huu ich, khong dung markdown, khong tu tao thong tin ngoai du lieu duoc cung cap.

            Du lieu showroom hien co:
            - Tong so xe: {dashboard.TotalCars}
            - So hang xe dang quan ly: {dashboard.BrandInventory.Count}
            - Ton kho theo hang: {brands}
            - Xe ban chay: {bestSellingCars}
            - Danh sach xe va gia: {carCatalog}

            Neu nguoi dung hoi ve gia xe, ton kho, xe theo hang hoac theo ten xe, hay tra loi dua tren danh sach xe va gia.
            Neu cau hoi vuot ngoai du lieu tren, hay noi ro gioi han va goi y nguoi dung hoi ve tong so xe, gia xe, ton kho, hang xe, hoac xe ban chay.

            Cau hoi nguoi dung: {userMessage}
            """;
    }

    private static string Normalize(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
