using MiniStore.DTOs.Auth;

namespace MiniStore.Services.Interfaces
{
    public interface IAuthService
    {
        Task ForgotPasswordAsync(ForgotPasswordRequestDto dto);
        Task<bool> VerifyOtpAsync(VerifyOtpRequestDto dto);
        Task ResetPasswordAsync(ResetPasswordRequestDto dto);
    }
}
