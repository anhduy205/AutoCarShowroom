using System.ComponentModel.DataAnnotations;
using AutoCarShowroom.Models;

namespace AutoCarShowroom.ViewModels
{
    public class CheckoutViewModel
    {
        public bool IsBuyNow { get; set; }

        public int? BuyNowCarId { get; set; }

        public IReadOnlyList<CartItemViewModel> Items { get; set; } = [];

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận thông tin.")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = OrderWorkflow.PaymentMethodSimulation;

        public decimal TotalAmount => Items.Sum(item => item.Price);
    }
}
