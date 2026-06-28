using MiniStore.DTOs;
using System.Collections.Generic;

namespace MiniStore.DTOs.Voucher
{
    public class ApplyVoucherRequestDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
        public int? UserId { get; set; }
        // Optional: include cart items to validate applicability and compute discounts on applicable items
        public List<CheckoutItemDto>? Items { get; set; } = new();
    }
}
