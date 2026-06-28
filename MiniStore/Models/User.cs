namespace MiniStore.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string? ResetPasswordOtp { get; set; }
        public DateTime? ResetPasswordOtpExpiry { get; set; }

    }

}
