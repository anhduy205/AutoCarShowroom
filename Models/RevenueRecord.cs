using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoCarShowroom.Models
{
    public class RevenueRecord
    {
        [Key]
        public int RevenueRecordId { get; set; }

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Doanh thu phải lớn hơn 0.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày ghi nhận.")]
        [Display(Name = "Ngày ghi nhận")]
        public DateTime ReceivedAt { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn nguồn doanh thu.")]
        [Display(Name = "Nguồn doanh thu")]
        public string SourceType { get; set; } = RevenueWorkflow.SourceTypeOther;

        public int? OrderId { get; set; }

        public int? BookingId { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        public Order? Order { get; set; }

        public Booking? Booking { get; set; }
    }
}
