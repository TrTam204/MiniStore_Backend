using System.ComponentModel.DataAnnotations;

namespace MiniStore.DTOs.Brand
{
    public class BrandUpdateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;
    }
}