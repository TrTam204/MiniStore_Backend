using Microsoft.EntityFrameworkCore;
using MiniStore.Data;
using MiniStore.DTOs;
using MiniStore.Models;
using MiniStore.Services.Interfaces;

namespace MiniStore.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        public OrderService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<string> CheckoutAsync(CheckoutRequestDto request)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                return "Giỏ hàng đang trống.";
            }
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return "Không tìm thấy người dùng.";
            }
            var order = new Order
            {
                UserId = request.UserId,
                OrderDate = DateTime.Now,
                TotalPrice = 0
            };
            foreach (var item in request.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product == null)
                {
                    return $"Không tìm thấy sản phẩm có id = {item.ProductId}.";
                }
                if (item.Quantity <= 0)
                {
                    return "Số lượng sản phẩm phải lớn hơn 0.";
                }
                if (product.Quantity < item.Quantity)
                {
                    return $"Sản phẩm {product.Name} không đủ số lượng.";
                }
                product.Quantity -= item.Quantity;
                var orderDetail = new OrderDetail
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    Price = product.SellPrice
                };
                order.TotalPrice += product.SellPrice * item.Quantity;
                order.OrderDetails.Add(orderDetail);
            }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return "Đặt hàng thành công.";
        }

        public async Task<List<OrderHistoryDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return orders.Select(o => new OrderHistoryDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Items = o.OrderDetails.Select(detail => new OrderHistoryItemDto
                    {
                        ProductId = detail.ProductId,
                        ProductName = detail.Product != null ? detail.Product.Name : "",
                        Quantity = detail.Quantity,
                        Price = detail.Price,
                        ImageUrl = detail.Product != null ? detail.Product.ImageUrl : "",
                    }).ToList()
                }).ToList();
        }
    }
}