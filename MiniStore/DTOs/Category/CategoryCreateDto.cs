using System.ComponentModel.DataAnnotations;

namespace MiniStore.DTOs.Category
{
    public class CategoryCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}