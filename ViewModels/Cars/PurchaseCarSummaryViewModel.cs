namespace AutoCarShowroom.ViewModels
{
    public class PurchaseCarSummaryViewModel
    {
        public int CarId { get; set; }

        public string CarName { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string ModelName { get; set; } = string.Empty;

        public string Image { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public bool CanPurchase { get; set; }

        public string AvailabilityMessage { get; set; } = string.Empty;
    }
}
