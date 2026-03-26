using AutoCarShowroom.Models;

namespace AutoCarShowroom.ViewModels
{
    public class HomeViewModel
    {
        public IReadOnlyList<Car> FeaturedCars { get; init; } = new List<Car>();

        public int TotalCars { get; init; }

        public decimal AveragePrice { get; init; }

        public int NewestModelYear { get; init; }
    }
}
