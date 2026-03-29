using AutoCarShowroom.Models;

namespace AutoCarShowroom.ViewModels
{
    public class OrderSuccessViewModel
    {
        public Order Order { get; set; } = null!;

        public IReadOnlyList<OrderItem> Items { get; set; } = [];
    }
}
