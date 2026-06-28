using MiniStore.DTOs.Voucher;
using MiniStore.Models;
using MiniStore.DTOs;
using System.Collections.Generic;
namespace MiniStore.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<ApplyVoucherResponseDto> ApplyPreviewAsync(string code, decimal orderAmount, List<CheckoutItemDto>? items = null, int? userId = null);
        Task<Voucher?> GetByCodeAsync(string code);
        Task<List<Voucher>> GetAllAsync();
        Task<List<Voucher>> GetActiveAsync();
        Task<Voucher?> GetByIdAsync(int id);
        Task<Voucher> CreateAsync(Voucher voucher);
        Task<bool> UpdateAsync(Voucher voucher);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleAsync(int id);
        Task<bool> IncrementUsedCountAsync(Voucher voucher);
    }
}
