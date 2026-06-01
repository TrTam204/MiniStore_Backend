using System.ComponentModel.DataAnnotations;

namespace MiniStore.DTOs.User
{
    public class UserUpdateDto
    {
        [Required]
        [MaxLength(50)]
        public string FullName { get; set; } = string.Empty;


        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;
        [Required]
        [MaxLength(150)]
        public string Address { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

}
