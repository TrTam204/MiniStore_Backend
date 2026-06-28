using Microsoft.EntityFrameworkCore;
using MiniStore.Data;
using MiniStore.DTOs;
using MiniStore.Models;
using MiniStore.Services.Interfaces;
using System.Text;

namespace MiniStore.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IVoucherService _voucherService;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderService> _logger;
        public OrderService(AppDbContext context, IVoucherService voucherService, IEmailService emailService, ILogger<OrderService> logger)
        {
            _context = context;
            _voucherService = voucherService;
            _emailService = emailService;
            _logger = logger;
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
                TotalPrice = 0,
                DiscountAmount = 0m,
                Status = request.PaymentMethod?.Trim().ToLower() == "qr" ? "Chờ xác nhận thanh toán" : "Chờ xác nhận",
                ShippingName = string.IsNullOrWhiteSpace(request.ShippingName) ? user.FullName : request.ShippingName,
                ShippingPhone = string.IsNullOrWhiteSpace(request.ShippingPhone) ? user.Phone : request.ShippingPhone,
                ShippingAddress = string.IsNullOrWhiteSpace(request.ShippingAddress) ? user.Address : request.ShippingAddress
            };
            decimal cartTotal = 0;
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
                cartTotal += product.SellPrice * item.Quantity;
                order.OrderDetails.Add(orderDetail);
            }
            var shippingFee = cartTotal < 500000 ? 30000 : 0;
            // Apply voucher if provided
            decimal discountAmount = 0m;
            if (!string.IsNullOrEmpty(request.VoucherCode))
            {
                var preview = await _voucherService.ApplyPreviewAsync(request.VoucherCode!, cartTotal, request.Items, request.UserId);
                if (preview.IsValid)
                {
                    discountAmount = preview.DiscountAmount;
                    var voucher = await _voucherService.GetByCodeAsync(request.VoucherCode!);
                    if (voucher != null)
                    {
                        order.VoucherId = voucher.Id;
                        order.VoucherCode = voucher.Code;
                        // increment used count (attempt)
                        var incOk = await _voucherService.IncrementUsedCountAsync(voucher);
                        if (!incOk)
                        {
                            // if cannot increment, reject
                            return "Voucher không còn lượt sử dụng.";
                        }
                    }
                }
                else
                {
                    return preview.Message;
                }
            }

            order.DiscountAmount = discountAmount;
            order.TotalPrice = cartTotal + shippingFee;
            order.FinalAmount = order.TotalPrice - discountAmount;
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
                    DiscountAmount = o.DiscountAmount,
                    FinalAmount = o.FinalAmount,
                    VoucherCode = o.VoucherCode,
                    ShippingFee = Math.Max(o.TotalPrice - o.OrderDetails.Sum(d => d.Price * d.Quantity), 0),
                    Status = o.Status,
                    ShippingName = o.ShippingName,
                    ShippingPhone = o.ShippingPhone,
                    ShippingAddress = o.ShippingAddress,
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

        public async Task<List<OrderHistoryDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return orders.Select(o => new OrderHistoryDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    DiscountAmount = o.DiscountAmount,
                    FinalAmount = o.FinalAmount,
                    VoucherCode = o.VoucherCode,
                    ShippingFee = Math.Max(o.TotalPrice - o.OrderDetails.Sum(d => d.Price * d.Quantity), 0),
                    Status = o.Status,
                    BuyerName = o.User != null ? o.User.FullName : string.Empty,
                    ShippingName = o.ShippingName,
                    ShippingPhone = o.ShippingPhone,
                    ShippingAddress = o.ShippingAddress,
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

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return false;
            }

            var finalStates = new[] { "Đã nhận", "Đã hủy", "Hoàn thành" };
            if (finalStates.Contains(order.Status))
            {
                return false;
            }

            var previousStatus = order.Status;
            order.Status = status;
            if (status == "Đã hủy" && previousStatus != "Đã hủy")
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = detail.Product ?? await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.Quantity += detail.Quantity;
                    }
                }
            }

            var deliveryEmail = string.Empty;
            var deliverySubject = string.Empty;
            var deliveryHtml = string.Empty;
            if ((status == "Đã nhận" || status == "Hoàn thành") && previousStatus != status)
            {
                var user = await _context.Users.FindAsync(order.UserId);
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    deliveryEmail = user.Email;
                    deliverySubject = $"[MiniStore] Đơn hàng #{order.Id} đã giao thành công!";
                    deliveryHtml = BuildOrderCompletedHtml(order, user);
                }
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(deliveryEmail) && !string.IsNullOrWhiteSpace(deliveryHtml))
            {
                var to = deliveryEmail;
                var subject = deliverySubject;
                var html = deliveryHtml;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendEmailAsync(to, subject, html);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to send delivery completed email for order {OrderId} to {Email}", order.Id, to);
                    }
                });
            }

            return true;
        }

        private string BuildOrderCompletedHtml(Order order, User user)
        {
            var itemRows = new StringBuilder();
            foreach (var detail in order.OrderDetails)
            {
                var productName = detail.Product?.Name ?? "Sản phẩm";
                itemRows.Append($@"<tr>
                    <td style='padding:8px;border:1px solid #ddd'>{productName}</td>
                    <td style='padding:8px;border:1px solid #ddd;text-align:center'>{detail.Quantity}</td>
                    <td style='padding:8px;border:1px solid #ddd;text-align:right'>{detail.Price:N0} đ</td>
                </tr>");
            }

            return $@"<div style='font-family:Arial, sans-serif; color:#333; line-height:1.6;'>
                <h2 style='color:#2e7d32;'>Đơn hàng #{order.Id} đã giao thành công</h2>
                <p>Xin chào {user.FullName},</p>
                <p>Cảm ơn bạn đã mua sắm tại MiniStore. Đơn hàng của bạn đã được giao thành công.</p>
                <h3 style='margin-top:24px;'>Chi tiết đơn hàng</h3>
                <table style='width:100%;border-collapse:collapse;margin-top:12px;'>
                    <thead>
                        <tr style='background:#f5f5f5;'>
                            <th style='padding:10px;border:1px solid #ddd;text-align:left;'>Sản phẩm</th>
                            <th style='padding:10px;border:1px solid #ddd;text-align:center;'>Số lượng</th>
                            <th style='padding:10px;border:1px solid #ddd;text-align:right;'>Giá</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemRows}
                    </tbody>
                </table>
                <p style='margin-top:16px;font-weight:600;'>Tổng thanh toán: {order.FinalAmount:N0} đ</p>
                <div style='margin-top:16px;padding:16px;background:#f9f9f9;border:1px solid #e0e0e0;'>
                    <p style='margin:0;font-weight:600;'>Người nhận hàng:</p>
                    <p style='margin:4px 0 0 0;'>{order.ShippingName ?? user.FullName}</p>
                    <p style='margin:4px 0 0 0;'>SĐT: {order.ShippingPhone ?? user.Phone}</p>
                    <p style='margin:4px 0 0 0;'>Địa chỉ nhận hàng: {order.ShippingAddress ?? user.Address}</p>
                </div>
                <p style='margin-top:24px;'>Nếu bạn có thắc mắc, xin vui lòng liên hệ với chúng tôi.</p>
                <p style='margin-top:16px;'>Trân trọng,<br/>Đội ngũ MiniStore</p>
            </div>";
        }
    }
}