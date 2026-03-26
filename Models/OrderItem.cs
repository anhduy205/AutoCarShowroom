using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoCarShowroom.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int CarId { get; set; }

        [Required]
        [Display(Name = "Tên xe")]
        public string CarName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Ảnh xe")]
        public string CarImage { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal UnitPrice { get; set; }

        public Order Order { get; set; } = null!;

        public Car? Car { get; set; }
    }
}
