using MiniStore.DTOs.Product;
using MiniStore.Models;

namespace MiniStore.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponseDto> CreateAsync(ProductCreateDto dto);
        Task<List<ProductResponseDto>> GetAllAsync();
        Task<ProductResponseDto?> GetByIdAsync(int id);
        Task<ProductResponseDto?> UpdateAsync(int id, ProductUpdateDto dto);
        Task<bool> DeleteAsync(int id);

    }
}