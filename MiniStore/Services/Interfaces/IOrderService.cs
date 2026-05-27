using MiniStore.DTOs;
namespace MiniStore.Services.Interfaces
{
    public interface IOrderService
    {
        Task<string> CheckoutAsync(CheckoutRequestDto request);
        Task<List<OrderHistoryDto>> GetOrdersByUserIdAsync(int userId);
    }
}