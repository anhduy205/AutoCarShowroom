using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoCarShowroom.Models
{
    public class Car
    {
        [Key]
        public int CarID { get; set; }

        [Required(ErrorMessage = "Vui long chon hang xe.")]
        [Display(Name = "Hang xe")]
        [StringLength(120)]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long nhap ten xe.")]
        [Display(Name = "Ten xe")]
        public string CarName { get; set; } = string.Empty;

        [Display(Name = "Gia ban")]
        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Gia ban phai lon hon hoac bang 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "Nam san xuat")]
        [Range(1950, 2100, ErrorMessage = "Nam san xuat khong hop le.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Vui long nhap duong dan hinh anh.")]
        [Display(Name = "Hinh anh")]
        public string Image { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long nhap mo ta.")]
        [Display(Name = "Mo ta")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;
    }
}
