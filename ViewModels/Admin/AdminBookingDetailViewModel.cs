using AutoCarShowroom.Models;

namespace AutoCarShowroom.ViewModels
{
    public class AdminBookingDetailViewModel
    {
        public Booking Booking { get; set; } = null!;

        public IReadOnlyList<string> BookingStatusOptions { get; set; } = [];
    }
}
