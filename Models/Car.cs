using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoCarShowroom.Models
{
    public class Car
    {
        [Key]
        public int CarID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên xe.")]
        [Display(Name = "Tên xe hiển thị")]
        public string CarName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập hãng xe.")]
        [Display(Name = "Hãng xe")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập dòng xe.")]
        [Display(Name = "Dòng xe")]
        public string ModelName { get; set; } = string.Empty;

        [Display(Name = "Giá bán")]
        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "Năm sản xuất")]
        [Range(1950, 2100, ErrorMessage = "Năm sản xuất không hợp lệ.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập màu sắc.")]
        [Display(Name = "Màu sắc")]
        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại xe.")]
        [Display(Name = "Loại xe")]
        public string BodyType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn trạng thái xe.")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đường dẫn hình ảnh.")]
        [Display(Name = "Hình ảnh")]
        public string Image { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập thông tin chung.")]
        [Display(Name = "Thông tin chung")]
        [DataType(DataType.MultilineText)]
        public string Specifications { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả nổi bật.")]
        [Display(Name = "Mô tả nổi bật")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập thông tin động cơ và khung xe.")]
        [Display(Name = "Động cơ & khung xe")]
        [DataType(DataType.MultilineText)]
        public string EngineAndChassis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập thông tin ngoại thất.")]
        [Display(Name = "Ngoại thất")]
        [DataType(DataType.MultilineText)]
        public string Exterior { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập thông tin nội thất.")]
        [Display(Name = "Nội thất")]
        [DataType(DataType.MultilineText)]
        public string Interior { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập thông tin ghế.")]
        [Display(Name = "Ghế")]
        [DataType(DataType.MultilineText)]
        public string Seats { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập thông tin tiện nghi.")]
        [Display(Name = "Tiện nghi")]
        [DataType(DataType.MultilineText)]
        public string Convenience { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập thông tin an ninh và chống trộm.")]
        [Display(Name = "An ninh / hệ thống chống trộm")]
        [DataType(DataType.MultilineText)]
        public string SecurityAndAntiTheft { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập thông tin an toàn chủ động.")]
        [Display(Name = "An toàn chủ động")]
        [DataType(DataType.MultilineText)]
        public string ActiveSafety { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập thông tin an toàn bị động.")]
        [Display(Name = "An toàn bị động")]
        [DataType(DataType.MultilineText)]
        public string PassiveSafety { get; set; } = string.Empty;
    }
}
