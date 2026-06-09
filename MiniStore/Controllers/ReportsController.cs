using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniStore.Services.Interfaces;

namespace MiniStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport()
        {
            var result = await _reportService.GetSalesReportAsync();
            return Ok(result);
        }
    }
}