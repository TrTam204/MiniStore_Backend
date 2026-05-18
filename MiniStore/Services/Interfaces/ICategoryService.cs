using MiniStore.DTOs.Category;

namespace MiniStore.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto);

        Task<List<CategoryResponseDto>> GetAllAsync();

        Task<CategoryResponseDto?> GetByIdAsync(int id);

        Task<CategoryResponseDto?> UpdateAsync(int id, CategoryUpdateDto dto);

        Task<bool> DeleteAsync(int id);
    }
}