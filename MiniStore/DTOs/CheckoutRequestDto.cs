namespace MiniStore.DTOs
{
    public class CheckoutRequestDto
    {
        public int UserId { get; set; }
        public List<CheckoutItemDto> Items { get; set; } = new();
    }
    public class CheckoutItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
