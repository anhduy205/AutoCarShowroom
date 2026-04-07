using System.ComponentModel.DataAnnotations;
using AutoCarShowroom.Models;
using AutoCarShowroom.Validation;

namespace AutoCarShowroom.ViewModels
{
    public class BookingCreateViewModel
    {
        public int CarId { get; set; }

        public PurchaseCarSummaryViewModel Car { get; set; } = new();

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

        [Required(ErrorMessage = "Vui lòng chọn loại dịch vụ.")]
        [Display(Name = "Loại dịch vụ")]
        public string ServiceType { get; set; } = BookingWorkflow.ServiceViewing;

        [Required(ErrorMessage = "Vui lòng chọn ngày giờ hẹn.")]
        [Display(Name = "Ngày giờ hẹn")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentAt { get; set; } = DateTime.Now.Date.AddDays(1).AddHours(9);

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }
    }
}
