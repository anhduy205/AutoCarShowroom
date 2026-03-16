using System.ComponentModel.DataAnnotations;

namespace AutoCarShowroom.Models
{
    public class Car
    {
        [Key]
        public int CarID { get; set; }

        [Required]
        public string CarName { get; set; }

        public decimal Price { get; set; }

        public int Year { get; set; }

        public string Image { get; set; }

        public string Description { get; set; }
    }
}