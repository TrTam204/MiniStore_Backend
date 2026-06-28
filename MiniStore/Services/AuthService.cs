using Microsoft.EntityFrameworkCore;
using MiniStore.Data;
using MiniStore.DTOs.Auth;
using MiniStore.Models;
using MiniStore.Services.Interfaces;
using System.Text;

namespace MiniStore.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IEmailService emailService, ILogger<AuthService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null)
                return;

            var rng = new Random();
            var otp = rng.Next(100000, 999999).ToString();
            user.ResetPasswordOtp = otp;
            user.ResetPasswordOtpExpiry = DateTime.UtcNow.AddMinutes(5);

            await _context.SaveChangesAsync();

            var html = $@"<div style='font-family: Arial, sans-serif; font-size:14px;'>
                <h3>MiniStore - Yêu cầu đặt lại mật khẩu</h3>
                <p>Mã OTP của bạn là: <strong style='font-size:18px'>{otp}</strong></p>
                <p>Mã này có hiệu lực trong 5 phút.</p>
                <p>Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.</p>
                </div>";

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(user.Email, "Yêu cầu đặt lại mật khẩu - MiniStore", html);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to send forgot-password email to {Email}", user.Email);
                }
            });
        }

        public async Task<bool> VerifyOtpAsync(VerifyOtpRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null) return false;
            if (user.ResetPasswordOtp == null) return false;
            if (user.ResetPasswordOtpExpiry == null || user.ResetPasswordOtpExpiry < DateTime.UtcNow) return false;
            return user.ResetPasswordOtp == dto.Otp;
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null) throw new Exception("Email không tồn tại");
            if (user.ResetPasswordOtp == null || user.ResetPasswordOtp != dto.Otp) throw new Exception("Mã OTP không đúng");
            if (user.ResetPasswordOtpExpiry == null || user.ResetPasswordOtpExpiry < DateTime.UtcNow) throw new Exception("Mã OTP đã hết hạn");

            user.PasswordHash = dto.NewPassword;
            user.ResetPasswordOtp = null;
            user.ResetPasswordOtpExpiry = null;

            await _context.SaveChangesAsync();
        }
    }
}
