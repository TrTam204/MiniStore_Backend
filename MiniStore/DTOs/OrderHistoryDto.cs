namespace MiniStore.DTOs
{
    public class OrderHistoryDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string? VoucherCode { get; set; }
        public decimal ShippingFee { get; set; }
        public string Status { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string? ShippingName { get; set; }
        public string? ShippingPhone { get; set; }
        public string? ShippingAddress { get; set; }
        public List<OrderHistoryItemDto> Items { get; set; } = new();
    }

    public class OrderHistoryItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}