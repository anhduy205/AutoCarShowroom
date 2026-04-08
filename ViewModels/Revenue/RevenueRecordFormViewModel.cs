using System.ComponentModel.DataAnnotations;
using AutoCarShowroom.Models;

namespace AutoCarShowroom.ViewModels
{
    public class RevenueRecordFormViewModel
    {
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335",
            ConvertValueInInvariantCulture = true,
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "Doanh thu phải lớn hơn 0.")]
        [Display(Name = "Số tiền")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày ghi nhận.")]
        [Display(Name = "Ngày ghi nhận")]
        [DataType(DataType.DateTime)]
        public DateTime ReceivedAt { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn nguồn doanh thu.")]
        [Display(Name = "Nguồn doanh thu")]
        public string SourceType { get; set; } = RevenueWorkflow.SourceTypeOther;

        [Display(Name = "Đơn mua xe")]
        public int? OrderId { get; set; }

        [Display(Name = "Lịch hẹn")]
        public int? BookingId { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }
    }
}
