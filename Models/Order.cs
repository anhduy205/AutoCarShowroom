using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoCarShowroom.Validation;

namespace AutoCarShowroom.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [StringLength(40)]
        [Display(Name = "Mã đơn hàng")]
        public string OrderCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        [RegularExpression(VietnamesePhoneNumberRules.Pattern, ErrorMessage = VietnamesePhoneNumberRules.ErrorMessage)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        [Required]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = OrderWorkflow.PaymentMethodQr;

        [Required]
        [Display(Name = "Trạng thái đơn hàng")]
        public string OrderStatus { get; set; } = OrderWorkflow.OrderStatusNew;

        [Required]
        [Display(Name = "Trạng thái thanh toán")]
        public string PaymentStatus { get; set; } = OrderWorkflow.PaymentStatusUnpaid;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> Items { get; set; } = [];
    }
}
