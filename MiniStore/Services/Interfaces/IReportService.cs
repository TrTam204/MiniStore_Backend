using MiniStore.DTOs.Report;

namespace MiniStore.Services.Interfaces
{
    public interface IReportService
    {
        Task<SalesReportSummaryDto> GetSalesReportAsync();
    }
}