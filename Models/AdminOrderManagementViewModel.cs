namespace AutoCarShowroom.Models
{
    public class AdminOrderManagementViewModel
    {
        public Order Order { get; set; } = null!;

        public IReadOnlyList<string> OrderStatusOptions { get; set; } = [];

        public IReadOnlyList<string> PaymentStatusOptions { get; set; } = [];
    }
}
