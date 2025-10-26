using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CafePOS.API.DTOs;
using CafePOS.API.Services;
using CafePOS.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CafePOS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "FinanceOfficer,Admin")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("daily/{date}")]
        public async Task<ActionResult<DailySummaryDto>> GetDailySummary(string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsedDate))
                    return BadRequest(new { message = "Invalid date format" });

                var summary = await _reportService.GetDailySummaryAsync(parsedDate);
                return Ok(summary);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpGet("top-items")]
        public async Task<IActionResult> GetTopSellingItems(DateTime startDate, DateTime endDate, int count = 10)
        {
            var items = await _reportService.GetTopSellingItemsAsync(startDate, endDate, count);
            return Ok(items);
        }

    }
}
