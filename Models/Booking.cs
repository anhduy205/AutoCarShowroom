using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoCarShowroom.Validation;

namespace AutoCarShowroom.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        [StringLength(40)]
        [Display(Name = "Mã lịch hẹn")]
        public string BookingCode { get; set; } = string.Empty;

        [Required]
        public int CarId { get; set; }

        [Required]
        [Display(Name = "Tên xe")]
        public string CarName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Ảnh xe")]
        public string CarImage { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá tham khảo")]
        public decimal QuotedPrice { get; set; }

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

        [Required(ErrorMessage = "Vui lòng chọn ngày giờ hẹn.")]
        [Display(Name = "Ngày giờ hẹn")]
        public DateTime AppointmentAt { get; set; }

        [Display(Name = "Ghi chú của khách")]
        public string? Note { get; set; }

        [Required]
        [Display(Name = "Trạng thái lịch hẹn")]
        public string BookingStatus { get; set; } = BookingWorkflow.StatusNew;

        [Display(Name = "Ghi chú quản trị")]
        public string? AdminNote { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Car? Car { get; set; }
    }
}
