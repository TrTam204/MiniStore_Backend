namespace MiniStore.DTOs.Report
{
    public class CategoryReportDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
