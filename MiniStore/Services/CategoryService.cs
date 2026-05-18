using Microsoft.EntityFrameworkCore;
using MiniStore.Data;
using MiniStore.DTOs.Category;
using MiniStore.Models;
using MiniStore.Services.Interfaces;

namespace MiniStore.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        public CategoryService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto)
        {
            var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower());
            if (existingCategory != null)
            {
                throw new Exception("Category name already exists");
            }

            var category = new Category
            {
                Name = dto.Name
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            var response = new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name
            };
            return response;
        }
        public async Task<List<CategoryResponseDto>> GetAllAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            var response = categories.Select(category => new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name
            }).ToList();
            return response;
        }
        public async Task<CategoryResponseDto?> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return null;
            }
            var response = new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name
            };
            return response;
        }
        public async Task<CategoryResponseDto?> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return null;
            }
            category.Name = dto.Name;
            await _context.SaveChangesAsync();

            var response = new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name
            };
            return response;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return false;
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}