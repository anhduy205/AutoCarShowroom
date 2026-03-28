namespace AutoCarShowroom.ViewModels
{
    public class HomeViewModel
    {
        public IReadOnlyList<CarLineCardViewModel> FeaturedLines { get; init; } = [];

        public CarLineCardViewModel? HeroLine { get; init; }

        public int TotalCars { get; init; }

        public int TotalLines { get; init; }

        public decimal AveragePrice { get; init; }

        public int NewestModelYear { get; init; }
    }
}
