using Microsoft.EntityFrameworkCore;
using MiniStore.Data;
using MiniStore.DTOs.Report;
using MiniStore.Services.Interfaces;

namespace MiniStore.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SalesReportSummaryDto> GetSalesReportAsync()
        {
            var items = await (
                from orderDetail in _context.OrderDetails
                join order in _context.Orders on orderDetail.OrderId equals order.Id
                join user in _context.Users on order.UserId equals user.Id
                join product in _context.Products on orderDetail.ProductId equals product.Id
                join category in _context.Categories on product.CategoryId equals category.Id into categoryJoin
                from category in categoryJoin.DefaultIfEmpty()
                orderby order.OrderDate descending, order.Id descending
                select new SalesReportDto
                {
                    OrderId = order.Id,
                    OrderDate = order.OrderDate,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    CategoryName = category != null ? category.Name : string.Empty,
                    Quantity = orderDetail.Quantity,
                    Price = orderDetail.Price,
                    TotalAmount = orderDetail.Price * orderDetail.Quantity
                }
            ).ToListAsync();

            return new SalesReportSummaryDto
            {
                TotalOrders = items.Select(item => item.OrderId).Distinct().Count(),
                TotalProductsSold = items.Sum(item => item.Quantity),
                TotalRevenue = items.Sum(item => item.TotalAmount),
                Items = items
            };
        }
    }
}