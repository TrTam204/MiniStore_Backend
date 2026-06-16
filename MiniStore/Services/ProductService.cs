using Microsoft.EntityFrameworkCore;
using MiniStore.Data;
using MiniStore.DTOs.Product;
using MiniStore.Models;
using MiniStore.Services.Interfaces;
using System.IO;
using System.Text.RegularExpressions;

namespace MiniStore.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        private string ProcessImageUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return imageUrl;

            var match = Regex.Match(imageUrl, @"^data:image\/[a-zA-Z]+;base64,(.+)$");
            if (!match.Success)
                return imageUrl;

            try
            {
                var base64Data = match.Groups[1].Value;
                var imageBytes = Convert.FromBase64String(base64Data);
                var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                var fileName = $"{Guid.NewGuid():N}.jpg";
                var imagePath = Path.Combine(imagesFolder, fileName);
                File.WriteAllBytes(imagePath, imageBytes);
                return $"/images/{fileName}";
            }
            catch
            {
                return imageUrl;
            }
        }

        public async Task<ProductResponseDto> CreateAsync(ProductCreateDto dto)
        {
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Name.ToLower() == dto.Name.ToLower());
            if (existingProduct != null)
            {
                throw new Exception("Tên sản phẩm này đã tồn tại");
            }

            var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                throw new Exception("Danh mục này không tồn tại");
            }
            if (dto.ImportPrice > dto.SellPrice)
            {
                Console.WriteLine("Warning: giá nhập đang lớn hơn giá bán !!!");
            }
            var product = new Product
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                SellPrice = dto.SellPrice,
                ImportPrice = dto.ImportPrice,
                Quantity = dto.Quantity,
                ImageUrl = ProcessImageUrl(dto.ImageUrl),
                Description = dto.Description
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            var response = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                SellPrice = product.SellPrice,
                ImportPrice = product.ImportPrice,
                Quantity = product.Quantity,
                ImageUrl = product.ImageUrl,
                Description = product.Description
            };
            return response;
        }
        public async Task<List<ProductResponseDto>> GetAllAsync()
        {
            var response = await _context.Products
                .Include(product => product.Category)
                .Include(product => product.Brand)
                .Select(product => new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category != null ? product.Category.Name : string.Empty,
                    BrandId = product.BrandId,
                    BrandName = product.Brand != null ? product.Brand.Name : string.Empty,
                    SellPrice = product.SellPrice,
                    ImportPrice = product.ImportPrice,
                    Quantity = product.Quantity,
                    ImageUrl = product.ImageUrl,
                    Description = product.Description
                })
                .ToListAsync();
                  return response;
        }
         public async Task<ProductResponseDto?> GetByIdAsync(int id)
        {
            var response = await _context.Products
                .Where(product => product.Id == id)
                .Include(product => product.Category)
                .Include(product => product.Brand)
                .Select(product => new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category != null ? product.Category.Name : string.Empty,
                    BrandId = product.BrandId,
                    BrandName = product.Brand != null ? product.Brand.Name : string.Empty,
                    SellPrice = product.SellPrice,
                    ImportPrice = product.ImportPrice,
                    Quantity = product.Quantity,
                    ImageUrl = product.ImageUrl,
                    Description = product.Description
                })
                .FirstOrDefaultAsync();

            return response;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetRelatedProductsAsync(int categoryId, int excludeProductId)
        {
            return await _context.Products
                .Where(product => product.CategoryId == categoryId && product.Id != excludeProductId)
                .Include(product => product.Category)
                .Include(product => product.Brand)
                .Select(product => new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category != null ? product.Category.Name : string.Empty,
                    BrandId = product.BrandId,
                    BrandName = product.Brand != null ? product.Brand.Name : string.Empty,
                    SellPrice = product.SellPrice,
                    ImportPrice = product.ImportPrice,
                    Quantity = product.Quantity,
                    ImageUrl = product.ImageUrl,
                    Description = product.Description
                })
                .Take(4)
                .ToListAsync();
        }

        public async Task<ProductResponseDto?> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p =>
            p.Name.ToLower() == dto.Name.ToLower()
            && p.Id != id);

            if (existingProduct != null)
            {
                throw new Exception("Tên sản phẩm đã tồn tại");
            }
            var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                throw new Exception("Danh mục này không tồn tại");
            }
            if (dto.ImportPrice > dto.SellPrice)
            {
                Console.WriteLine("Warning: giá nhập đang lớn hơn giá bán !!!");
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return null;
            }
            product.Name = dto.Name;
            product.CategoryId = dto.CategoryId;
            product.SellPrice = dto.SellPrice;
            product.ImportPrice = dto.ImportPrice;
            product.Quantity = dto.Quantity;
            product.BrandId = dto.BrandId;
            product.ImageUrl = ProcessImageUrl(dto.ImageUrl);
            product.Description = dto.Description;
            await _context.SaveChangesAsync();
            var response = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                CategoryId = product.CategoryId,
                SellPrice = product.SellPrice,
                ImportPrice = product.ImportPrice,
                Quantity = product.Quantity,
                ImageUrl = product.ImageUrl,
                Description = product.Description
            };
            return response;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return false;
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}