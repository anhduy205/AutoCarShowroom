using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AutoCarShowroom.ViewModels
{
    public class CarFormViewModel
    {
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
        public decimal Price { get; set; }

        [Display(Name = "Năm sản xuất")]
        [Range(1950, 2100, ErrorMessage = "Năm sản xuất không hợp lệ.")]
        public int Year { get; set; } = DateTime.Now.Year;

        [Required(ErrorMessage = "Vui lòng nhập màu sắc.")]
        [Display(Name = "Màu sắc")]
        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại xe.")]
        [Display(Name = "Loại xe")]
        public string BodyType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn trạng thái xe.")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Còn hàng";

        [Required(ErrorMessage = "Vui lòng nhập mô tả.")]
        [Display(Name = "Mô tả nổi bật")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập thông số kỹ thuật.")]
        [Display(Name = "Thông số kỹ thuật")]
        [DataType(DataType.MultilineText)]
        public string Specifications { get; set; } = string.Empty;

        [Display(Name = "Ảnh xe")]
        public IFormFile? ImageFile { get; set; }

        public string CurrentImagePath { get; set; } = string.Empty;
    }
}
