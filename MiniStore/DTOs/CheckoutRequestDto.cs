namespace MiniStore.DTOs
{
    public class CheckoutRequestDto
    {
        public int UserId { get; set; }
        public List<CheckoutItemDto> Items { get; set; } = new();
        public string? VoucherCode { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ShippingName { get; set; }
        public string? ShippingPhone { get; set; }
        public string? ShippingAddress { get; set; }
    }
    public class CheckoutItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
