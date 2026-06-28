namespace MiniStore.DTOs.Report
{
    public class SalesReportDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProductNames { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}