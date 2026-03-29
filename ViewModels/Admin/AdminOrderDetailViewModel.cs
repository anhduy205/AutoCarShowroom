using AutoCarShowroom.Models;

namespace AutoCarShowroom.ViewModels
{
    public class AdminOrderDetailViewModel
    {
        public Order Order { get; set; } = null!;

        public IReadOnlyList<string> OrderStatusOptions { get; set; } = [];
    }
}
