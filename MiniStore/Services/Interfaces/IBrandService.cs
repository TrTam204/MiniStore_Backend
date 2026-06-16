using MiniStore.DTOs.Brand;

namespace MiniStore.Services.Interfaces
{
    public interface IBrandService
    {
        Task<BrandResponseDto> CreateAsync(BrandCreateDto dto);
        Task<List<BrandResponseDto>> GetAllAsync();
        Task<BrandResponseDto?> GetByIdAsync(int id);
        Task<BrandResponseDto?> UpdateAsync(int id, BrandUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}