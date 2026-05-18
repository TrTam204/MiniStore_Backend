using System.ComponentModel.DataAnnotations;

namespace MiniStore.DTOs.Product
{
    public class ProductCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal SellPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ImportPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}
