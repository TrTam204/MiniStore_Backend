using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniStore.DTOs;
using MiniStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace MiniStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly MiniStore.Services.Interfaces.IInvoiceService _invoiceService;
        public OrdersController(IOrderService orderService, MiniStore.Services.Interfaces.IInvoiceService invoiceService)
        {
            _orderService = orderService;
            _invoiceService = invoiceService;
        }

        [Authorize]
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto request)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (tokenUserId != request.UserId.ToString())
            {
                return Forbid();
            }
            var result = await _orderService.CheckoutAsync(request);
            if (result != "Đặt hàng thành công.")
            {
                return BadRequest(result);
            }
            return Ok(result);
    }
        [Authorize]
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetOrderHistory(int userId)
        {
            var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (tokenUserId != userId.ToString())
            {
                return Forbid();
            }
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("status/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] string status)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, status);
            if (!success)
            {
                return NotFound();
            }
            return Ok();
        }

        [Authorize]
        [HttpGet("{orderId}/invoice")]
        public async Task<IActionResult> GetInvoice(int orderId)
        {
            // user must be owner or admin
            var tokenUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                // check ownership
                if (tokenUserId == null) return Forbid();
                var orders = await _orderService.GetOrdersByUserIdAsync(int.Parse(tokenUserId));
                if (!orders.Any(o => o.OrderId == orderId)) return Forbid();
            }
            if (_invoiceService == null) return NotFound("Invoice service not available.");
            var bytes = await _invoiceService.GenerateInvoicePdfAsync(orderId);
            if (bytes == null || bytes.Length == 0) return NotFound();
            return File(bytes, "application/pdf", $"invoice_{orderId}.pdf");
        }
    }
}
