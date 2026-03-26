using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace AutoCarShowroom.Extensions
{
    public static class SessionCartExtensions
    {
        private const string CartKey = "cart:carIds";

        public static List<int> GetCartCarIds(this ISession session)
        {
            var rawValue = session.GetString(CartKey);

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<int>>(rawValue) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        public static void SetCartCarIds(this ISession session, IEnumerable<int> carIds)
        {
            var normalizedIds = carIds
                .Distinct()
                .ToList();

            if (normalizedIds.Count == 0)
            {
                session.Remove(CartKey);
                return;
            }

            session.SetString(CartKey, JsonSerializer.Serialize(normalizedIds));
        }

        public static bool AddCarToCart(this ISession session, int carId)
        {
            var currentIds = session.GetCartCarIds();

            if (currentIds.Contains(carId))
            {
                return false;
            }

            currentIds.Add(carId);
            session.SetCartCarIds(currentIds);
            return true;
        }

        public static bool RemoveCarFromCart(this ISession session, int carId)
        {
            var currentIds = session.GetCartCarIds();

            if (!currentIds.Remove(carId))
            {
                return false;
            }

            session.SetCartCarIds(currentIds);
            return true;
        }

        public static void ClearCart(this ISession session)
        {
            session.Remove(CartKey);
        }
    }
}
