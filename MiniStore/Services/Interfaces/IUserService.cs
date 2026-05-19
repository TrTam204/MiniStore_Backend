using MiniStore.DTOs.User;

namespace MiniStore.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto> CreateAsync(UserCreateDto dto);
        Task<List<UserResponseDto>> GetAllAsync();
        Task<UserResponseDto?> GetByIdAsync(int id);
        Task<UserResponseDto?> UpdateAsync(int id, UserUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<UserResponseDto> GetUserByEmailAsync(string email);
    }
}
