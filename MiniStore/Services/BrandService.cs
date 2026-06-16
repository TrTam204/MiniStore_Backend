using Microsoft.EntityFrameworkCore;
using MiniStore.Data;
using MiniStore.DTOs.Brand;
using MiniStore.Models;
using MiniStore.Services.Interfaces;

namespace MiniStore.Services
{
    public class BrandService : IBrandService
    {
        private readonly AppDbContext _context;
        public BrandService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BrandResponseDto> CreateAsync(BrandCreateDto dto)
        {
            var existingBrand = await _context.Brands
                .FirstOrDefaultAsync(b => b.Name.ToLower() == dto.Name.ToLower());
            if (existingBrand != null)
            {
                throw new Exception("Tên thương hiệu này đã tồn tại");
            }

            var brand = new Brand
            {
                Name = dto.Name,
                Description = dto.Description,
                Country = dto.Country
            };
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return new BrandResponseDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                Country = brand.Country
            };
        }

        public async Task<List<BrandResponseDto>> GetAllAsync()
        {
            return await _context.Brands
                .Select(b => new BrandResponseDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Country = b.Country
                })
                .ToListAsync();
        }

        public async Task<BrandResponseDto?> GetByIdAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            return brand == null ? null : new BrandResponseDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                Country = brand.Country
            };
        }

        public async Task<BrandResponseDto?> UpdateAsync(int id, BrandUpdateDto dto)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return null;
            }
            brand.Name = dto.Name;
            brand.Description = dto.Description;
            brand.Country = dto.Country;
            await _context.SaveChangesAsync();
            return new BrandResponseDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                Country = brand.Country
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return false;
            }
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}