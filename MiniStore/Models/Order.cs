namespace MiniStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        // Voucher related
        public int? VoucherId { get; set; }
        public string? VoucherCode { get; set; }
        public decimal DiscountAmount { get; set; } = 0m;
        public decimal FinalAmount { get; set; }
        public Voucher? Voucher { get; set; }
        public string Status { get; set; } = "Chờ xác nhận";
        public string? ShippingName { get; set; }
        public string? ShippingPhone { get; set; }
        public string? ShippingAddress { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set;} = new List<OrderDetail>();
    }
}
