using AutoCarShowroom.Models;

namespace AutoCarShowroom.ViewModels
{
    public class HomeViewModel
    {
        public IReadOnlyList<Car> FeaturedCars { get; init; } = [];

        public Car? HeroCar { get; init; }

        public int TotalCars { get; init; }

        public int TotalLines { get; init; }

        public decimal AveragePrice { get; init; }

        public int NewestModelYear { get; init; }
    }
}
