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
        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
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
    }
}
