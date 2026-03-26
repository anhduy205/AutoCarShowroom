namespace AutoCarShowroom.ViewModels
{
    public class CartViewModel
    {
        public IReadOnlyList<CartItemViewModel> Items { get; set; } = [];

        public decimal TotalAmount => Items.Sum(item => item.Price);

        public bool CanCheckout => Items.Count > 0 && Items.All(item => item.CanOrder);
    }
}
