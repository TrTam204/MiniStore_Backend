using System.ComponentModel.DataAnnotations;

namespace MiniStore.DTOs.Category
{
    public class CategoryUpdateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}

