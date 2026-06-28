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
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Category)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var items = orders.Select(order =>
            {
                var subtotal = order.OrderDetails.Sum(detail => detail.Price * detail.Quantity);
                var shippingFee = Math.Max(order.TotalPrice - subtotal, 0);
                var totalAmount = order.FinalAmount > 0
                    ? order.FinalAmount
                    : subtotal + shippingFee - order.DiscountAmount;

                var productNames = string.Join(", ", order.OrderDetails
                    .Select(detail => detail.Product?.Name ?? string.Empty)
                    .Where(name => !string.IsNullOrWhiteSpace(name)));

                var categoryName = string.Join(", ", order.OrderDetails
                    .Select(detail => detail.Product?.Category?.Name ?? string.Empty)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct());

                return new SalesReportDto
                {
                    OrderId = order.Id,
                    OrderDate = order.OrderDate,
                    UserId = order.User?.Id ?? 0,
                    FullName = order.User?.FullName ?? string.Empty,
                    Email = order.User?.Email ?? string.Empty,
                    ProductNames = string.IsNullOrWhiteSpace(productNames) ? "" : productNames,
                    CategoryName = string.IsNullOrWhiteSpace(categoryName) ? "" : categoryName,
                    TotalQuantity = order.OrderDetails.Sum(detail => detail.Quantity),
                    TotalAmount = totalAmount,
                    Status = order.Status
                };
            }).ToList();

            var categoryBreakdown = orders
                .SelectMany(order =>
                {
                    var orderSubtotal = order.OrderDetails.Sum(detail => detail.Price * detail.Quantity);
                    var subtotal = orderSubtotal;
                    var shippingFee = Math.Max(order.TotalPrice - subtotal, 0);
                    var orderTotal = order.FinalAmount > 0
                        ? order.FinalAmount
                        : subtotal + shippingFee - order.DiscountAmount;

                    return order.OrderDetails.Select(detail => new
                    {
                        Category = detail.Product?.Category?.Name ?? "Khác",
                        LineAmount = detail.Price * detail.Quantity,
                        Quantity = detail.Quantity,
                        OrderSubtotal = orderSubtotal,
                        OrderTotal = orderTotal
                    });
                })
                .GroupBy(x => x.Category)
                .Select(group => new CategoryReportDto
                {
                    CategoryName = group.Key,
                    TotalQuantity = group.Sum(x => x.Quantity),
                    TotalAmount = group.Sum(x => x.OrderSubtotal > 0
                        ? Math.Round(x.OrderTotal * (x.LineAmount / x.OrderSubtotal), 2)
                        : 0)
                })
                .ToList();

            return new SalesReportSummaryDto
            {
                TotalOrders = items.Count,
                TotalProductsSold = orders.Sum(order => order.OrderDetails.Sum(detail => detail.Quantity)),
                TotalRevenue = items.Sum(item => item.TotalAmount),
                Items = items,
                CategoryBreakdown = categoryBreakdown
            };
        }
    }
}