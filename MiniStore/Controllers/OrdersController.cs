using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniStore.DTOs;
using MiniStore.Services.Interfaces;

namespace MiniStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrdersController (IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto request)
        {
            var result = await _orderService.CheckoutAsync(request);
            if ( result !="Đặt hàng thành công.")
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetOrderHistory(int userId)
        {
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }
    }
}
