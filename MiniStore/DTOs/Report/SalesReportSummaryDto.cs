namespace MiniStore.DTOs.Report
{
    public class SalesReportSummaryDto
    {
        public int TotalOrders { get; set; }
        public int TotalProductsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<SalesReportDto> Items { get; set; } = new();
        public List<CategoryReportDto> CategoryBreakdown { get; set; } = new();
    }
}