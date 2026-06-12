namespace MiniStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Chờ xác nhận";
        public ICollection<OrderDetail> OrderDetails { get; set;} = new List<OrderDetail>();
    }
}
