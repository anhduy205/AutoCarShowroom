using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AutoCarShowroom.Models;

namespace AutoCarShowroom.Services.Chatbot
{
    public static partial class ChatbotTextParser
    {
        private static readonly CultureInfo VietnameseCulture = new("vi-VN");
        private static readonly HashSet<string> SearchStopWords = new(StringComparer.Ordinal)
        {
            "toi", "muon", "can", "xe", "nao", "loai", "kieu", "gi", "giup", "goi", "y",
            "tu", "van", "cho", "di", "de", "va", "la", "co", "nhung", "trong", "kho",
            "showroom", "hang", "ngay", "duoi", "tren", "tam", "khoang", "nhu", "cau",
            "anh", "chi", "em", "toi", "toi", "muon", "mot", "nhung", "cua", "theo"
        };

        public static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = ChatbotTextEncodingHelper.NormalizeText(value).Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
                if (unicodeCategory == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(character) || char.IsWhiteSpace(character))
                {
                    builder.Append(character);
                }
                else
                {
                    builder.Append(' ');
                }
            }

            return MultiSpaceRegex()
                .Replace(
                    builder
                        .ToString()
                        .Replace('đ', 'd')
                        .Replace('Đ', 'D')
                        .Normalize(NormalizationForm.FormC)
                        .ToLowerInvariant(),
                    " ")
                .Trim();
        }

        public static decimal? ExtractMoneyValue(string? message)
        {
            var normalizedMessage = Normalize(message);
            if (string.IsNullOrWhiteSpace(normalizedMessage))
            {
                return null;
            }

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

            var directAmountMatch = Regex.Match(normalizedMessage, @"\b\d{8,12}\b");
            if (directAmountMatch.Success && decimal.TryParse(directAmountMatch.Value, out var directValue))
            {
                return directValue;
            }

            return null;
        }

        public static ChatbotBudgetConstraint? ExtractBudgetConstraint(string? message)
        {
            var amount = ExtractMoneyValue(message);
            if (!amount.HasValue)
            {
                return null;
            }

            var normalizedMessage = Normalize(message);
            if (string.IsNullOrWhiteSpace(normalizedMessage))
            {
                return null;
            }

            if (ContainsAny(normalizedMessage, "tren", "tro len", "it nhat", "toi thieu") ||
                Regex.IsMatch(normalizedMessage, @"\btu\s+\d"))
            {
                return new ChatbotBudgetConstraint
                {
                    Amount = amount.Value,
                    MinBudget = amount.Value,
                    Mode = "over"
                };
            }

            if (ContainsAny(normalizedMessage, "khoang", "tam", "quanh", "gan"))
            {
                return new ChatbotBudgetConstraint
                {
                    Amount = amount.Value,
                    MaxBudget = amount.Value,
                    Mode = "around"
                };
            }

            return new ChatbotBudgetConstraint
            {
                Amount = amount.Value,
                MaxBudget = amount.Value,
                Mode = "under"
            };
        }

        public static int? ExtractTermMonths(string? message)
        {
            var normalizedMessage = Normalize(message);
            if (string.IsNullOrWhiteSpace(normalizedMessage))
            {
                return null;
            }

            var monthMatch = Regex.Match(normalizedMessage, @"(\d{1,3})\s*(thang)");
            if (monthMatch.Success && int.TryParse(monthMatch.Groups[1].Value, out var monthValue) && monthValue > 0)
            {
                return monthValue;
            }

            var yearMatch = Regex.Match(normalizedMessage, @"(\d{1,2})\s*(nam)");
            if (yearMatch.Success && int.TryParse(yearMatch.Groups[1].Value, out var yearValue) && yearValue > 0)
            {
                return yearValue * 12;
            }

            return null;
        }

        public static int? ExtractSeatCount(string? message)
        {
            var normalizedMessage = Normalize(message);
            if (string.IsNullOrWhiteSpace(normalizedMessage))
            {
                return null;
            }

            var seatMatch = Regex.Match(normalizedMessage, @"(\d{1,2})\s*cho");
            if (seatMatch.Success && int.TryParse(seatMatch.Groups[1].Value, out var seatCount) && seatCount > 0)
            {
                return seatCount;
            }

            return null;
        }

        public static string? ExtractPhoneNumber(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            var digits = new string(message.Where(char.IsDigit).ToArray());
            return digits.Length >= 10 ? digits[..10] : null;
        }

        public static string? ExtractEmail(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            var match = Regex.Match(message, @"[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase);
            return match.Success ? match.Value : null;
        }

        public static DateTime? ExtractAppointment(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            var rawMessage = message.Trim();
            var formats = new[]
            {
                "dd/MM/yyyy HH:mm",
                "d/M/yyyy HH:mm",
                "dd/MM/yyyy H:mm",
                "d/M/yyyy H:mm",
                "dd-MM-yyyy HH:mm",
                "d-M-yyyy HH:mm",
                "dd-MM-yyyy H:mm",
                "d-M-yyyy H:mm",
                "yyyy-MM-dd HH:mm",
                "yyyy-M-d HH:mm",
                "yyyy-MM-ddTHH:mm"
            };

            if (DateTime.TryParseExact(rawMessage, formats, VietnameseCulture, DateTimeStyles.AllowWhiteSpaces, out var exactDate))
            {
                return exactDate;
            }

            if (DateTime.TryParse(rawMessage, VietnameseCulture, DateTimeStyles.AllowWhiteSpaces, out var parsedDate))
            {
                return parsedDate;
            }

            var tomorrowMatch = Regex.Match(Normalize(rawMessage), @"mai\s+(\d{1,2})(?:h| gio)?(?:[: ](\d{2}))?");
            if (tomorrowMatch.Success && int.TryParse(tomorrowMatch.Groups[1].Value, out var hour))
            {
                var minute = 0;
                if (tomorrowMatch.Groups[2].Success)
                {
                    int.TryParse(tomorrowMatch.Groups[2].Value, out minute);
                }

                return DateTime.Today.AddDays(1).AddHours(hour).AddMinutes(minute);
            }

            return null;
        }

        public static string? ExtractBookingServiceType(string? message)
        {
            var normalizedMessage = Normalize(message);
            if (string.IsNullOrWhiteSpace(normalizedMessage))
            {
                return null;
            }

            if (ContainsAny(normalizedMessage, "lai thu", "thu lai", "dang ky lai thu"))
            {
                return BookingWorkflow.ServiceTestDrive;
            }

            if (ContainsAny(normalizedMessage, "tu van", "can tu van", "hoi them"))
            {
                return BookingWorkflow.ServiceConsultation;
            }

            if (ContainsAny(normalizedMessage, "xem xe", "xem truc tiep", "xem mau xe"))
            {
                return BookingWorkflow.ServiceViewing;
            }

            return null;
        }

        public static bool LooksLikeGreeting(string normalizedMessage)
        {
            if (string.IsNullOrWhiteSpace(normalizedMessage))
            {
                return false;
            }

            if (normalizedMessage.StartsWith("xin chao", StringComparison.Ordinal) ||
                normalizedMessage.StartsWith("chao ban", StringComparison.Ordinal) ||
                normalizedMessage.StartsWith("chao em", StringComparison.Ordinal))
            {
                return true;
            }

            var tokens = TokenizeNormalizedWords(normalizedMessage);
            if (tokens.Count == 1)
            {
                return tokens[0] is "chao" or "hello" or "hi" or "alo";
            }

            return false;
        }

        public static bool ContainsAny(string normalizedMessage, params string[] keywords)
        {
            return keywords.Any(keyword => normalizedMessage.Contains(keyword, StringComparison.Ordinal));
        }

        public static IReadOnlyList<string> TokenizeSearchKeywords(string? message)
        {
            var normalizedMessage = Normalize(message);
            if (string.IsNullOrWhiteSpace(normalizedMessage))
            {
                return [];
            }

            return normalizedMessage
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(token => token.Length >= 3 || int.TryParse(token, out _))
                .Where(token => !SearchStopWords.Contains(token))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        public static IReadOnlyList<string> TokenizeNormalizedWords(string? normalizedMessage)
        {
            if (string.IsNullOrWhiteSpace(normalizedMessage))
            {
                return [];
            }

            return normalizedMessage
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
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

        [GeneratedRegex(@"\s+")]
        private static partial Regex MultiSpaceRegex();
    }
}
