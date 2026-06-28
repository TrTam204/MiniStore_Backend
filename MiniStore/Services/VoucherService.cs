using Microsoft.EntityFrameworkCore;
using MiniStore.Data;
using MiniStore.DTOs.Voucher;
using MiniStore.Models;
using MiniStore.Services.Interfaces;
using MiniStore.DTOs;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace MiniStore.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly AppDbContext _context;
        public VoucherService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApplyVoucherResponseDto> ApplyPreviewAsync(string code, decimal orderAmount, List<CheckoutItemDto>? items = null, int? userId = null)
        {
            var resp = new ApplyVoucherResponseDto { IsValid = false, DiscountAmount = 0m, FinalAmount = orderAmount };
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == code);
            if (voucher == null)
            {
                resp.Message = "Voucher không tồn tại.";
                return resp;
            }
            if (!voucher.IsActive)
            {
                resp.Message = "Voucher không còn hoạt động.";
                return resp;
            }
            var now = DateTime.Now;
            if (now < voucher.StartDate || now > voucher.EndDate)
            {
                resp.Message = "Voucher chưa hiệu lực hoặc đã hết hạn.";
                return resp;
            }
            if (voucher.UsedCount >= voucher.Quantity)
            {
                resp.Message = "Voucher đã hết lượt sử dụng.";
                return resp;
            }
            // Check 1 lần/User: User chưa dùng mã này trước
            if (userId.HasValue)
            {
                var userOrders = await _context.Orders
                    .Where(o => o.UserId == userId.Value)
                    .ToListAsync();

                var userPreviousOrder = userOrders.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.VoucherCode) &&
                    o.VoucherCode == code &&
                    !string.Equals(o.Status, "Đã hủy", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(o.Status, "Cancelled", StringComparison.OrdinalIgnoreCase));

                if (userPreviousOrder != null)
                {
                    resp.Message = "Bạn đã sử dụng mã này rồi. Mỗi mã chỉ được sử dụng một lần.";
                    return resp;
                }
            }
            if (orderAmount < voucher.MinimumOrderAmount)
            {
                resp.Message = "Đơn hàng không đạt mức tối thiểu để áp dụng voucher.";
                return resp;
            }

            // Determine applicable subtotal depending on applicability rules
            decimal applicableSubtotal = orderAmount;
            if (voucher.ApplicableType == ApplicableType.ByCategory)
            {
                if (!voucher.ApplicableCategoryId.HasValue)
                {
                    resp.Message = "Voucher không được cấu hình danh mục áp dụng.";
                    return resp;
                }
                if (items == null || items.Count == 0)
                {
                    resp.Message = "Cần thông tin giỏ hàng để kiểm tra voucher áp dụng theo danh mục.";
                    return resp;
                }
                var productIds = items.Select(i => i.ProductId).ToList();
                var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
                applicableSubtotal = 0m;
                foreach (var it in items)
                {
                    var prod = products.FirstOrDefault(p => p.Id == it.ProductId);
                    if (prod != null && prod.CategoryId == voucher.ApplicableCategoryId)
                    {
                        applicableSubtotal += prod.SellPrice * it.Quantity;
                    }
                }
                if (applicableSubtotal <= 0)
                {
                    resp.Message = "Voucher không áp dụng cho sản phẩm trong giỏ hàng.";
                    return resp;
                }
            }
            else if (voucher.ApplicableType == ApplicableType.ByProducts)
            {
                if (string.IsNullOrEmpty(voucher.ApplicableProductIds))
                {
                    resp.Message = "Voucher không được cấu hình sản phẩm áp dụng.";
                    return resp;
                }
                if (items == null || items.Count == 0)
                {
                    resp.Message = "Cần thông tin giỏ hàng để kiểm tra voucher áp dụng theo sản phẩm.";
                    return resp;
                }
                List<int> allowed = new();
                try
                {
                    allowed = JsonSerializer.Deserialize<List<int>>(voucher.ApplicableProductIds) ?? new List<int>();
                }
                catch
                {
                    allowed = new List<int>();
                }
                if (allowed.Count == 0)
                {
                    resp.Message = "Voucher không có sản phẩm hợp lệ.";
                    return resp;
                }
                var productIds = items.Select(i => i.ProductId).ToList();
                var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();
                applicableSubtotal = 0m;
                foreach (var it in items)
                {
                    if (allowed.Contains(it.ProductId))
                    {
                        var prod = products.FirstOrDefault(p => p.Id == it.ProductId);
                        if (prod != null)
                        {
                            applicableSubtotal += prod.SellPrice * it.Quantity;
                        }
                    }
                }
                if (applicableSubtotal <= 0)
                {
                    resp.Message = "Voucher không áp dụng cho sản phẩm trong giỏ hàng.";
                    return resp;
                }
            }

            decimal discount = 0m;
            if (voucher.DiscountType == DiscountType.Percentage)
            {
                discount = Math.Round(applicableSubtotal * (voucher.DiscountValue / 100m), 0);
                if (voucher.MaximumDiscountAmount.HasValue && discount > voucher.MaximumDiscountAmount.Value)
                {
                    discount = voucher.MaximumDiscountAmount.Value;
                }
            }
            else
            {
                // fixed amount: if applicability limits the subtotal, cap discount to the applicable subtotal
                discount = Math.Min(voucher.DiscountValue, applicableSubtotal);
            }
            if (discount > orderAmount) discount = orderAmount;
            resp.IsValid = true;
            resp.DiscountAmount = discount;
            resp.FinalAmount = orderAmount - discount;
            resp.Message = "Áp dụng thành công";
            return resp;
        }

        public async Task<Voucher?> GetByCodeAsync(string code)
        {
            return await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == code);
        }

        public async Task<List<Voucher>> GetAllAsync()
        {
            return await _context.Vouchers.OrderByDescending(v => v.CreatedAt).ToListAsync();
        }

        public async Task<List<Voucher>> GetActiveAsync()
        {
            return await _context.Vouchers
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<Voucher?> GetByIdAsync(int id)
        {
            return await _context.Vouchers.FindAsync(id);
        }

        public async Task<Voucher> CreateAsync(Voucher voucher)
        {
            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();
            return voucher;
        }

        public async Task<bool> UpdateAsync(Voucher voucher)
        {
            var exist = await _context.Vouchers.FindAsync(voucher.Id);
            if (exist == null) return false;
            exist.Code = voucher.Code;
            exist.Description = voucher.Description;
            exist.DiscountType = voucher.DiscountType;
            exist.DiscountValue = voucher.DiscountValue;
            exist.MinimumOrderAmount = voucher.MinimumOrderAmount;
            exist.MaximumDiscountAmount = voucher.MaximumDiscountAmount;
            exist.StartDate = voucher.StartDate;
            exist.EndDate = voucher.EndDate;
            exist.Quantity = voucher.Quantity;
            exist.IsActive = voucher.IsActive;
            exist.ApplicableType = voucher.ApplicableType;
            exist.ApplicableCategoryId = voucher.ApplicableCategoryId;
            exist.ApplicableProductIds = voucher.ApplicableProductIds;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exist = await _context.Vouchers.FindAsync(id);
            if (exist == null) return false;
            _context.Vouchers.Remove(exist);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleAsync(int id)
        {
            var exist = await _context.Vouchers.FindAsync(id);
            if (exist == null) return false;
            exist.IsActive = !exist.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementUsedCountAsync(Voucher voucher)
        {
            if (voucher == null) return false;
            var exist = await _context.Vouchers.FindAsync(voucher.Id);
            if (exist == null) return false;
            if (exist.UsedCount >= exist.Quantity) return false;
            exist.UsedCount += 1;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
