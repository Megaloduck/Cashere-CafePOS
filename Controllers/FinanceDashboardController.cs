using CafePOS.API.DTOs;
using CafePOS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace CafePOS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "FinanceOfficer,Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IPaymentService _paymentService;

        public DashboardController(IReportService reportService, IPaymentService paymentService)
        {
            _reportService = reportService;
            _paymentService = paymentService;
        }

        [HttpGet("summary/today")]
        public async Task<ActionResult<DailySummaryDto>> GetTodaySummary()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var summary = await _reportService.GetDailySummaryAsync(today);
                return Ok(summary);
            }
            catch (KeyNotFoundException)
            {
                // Return empty summary if no data for today
                return Ok(new DailySummaryDto
                {
                    SummaryDate = DateTime.UtcNow.Date,
                    TotalTransactions = 0,
                    TotalRevenue = 0,
                    TotalTax = 0,
                    CashCollected = 0,
                    QRISCollected = 0,
                    TotalItemsSold = 0
                });
            }
        }

        [HttpGet("summary/date-range")]
        public async Task<ActionResult<List<DailySummaryDto>>> GetDateRangeSummary(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var summaries = new List<DailySummaryDto>();

                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    try
                    {
                        var summary = await _reportService.GetDailySummaryAsync(date);
                        summaries.Add(summary);
                    }
                    catch (KeyNotFoundException)
                    {
                        // Skip dates with no data
                        continue;
                    }
                }

                return Ok(summaries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("transactions/recent")]
        public async Task<ActionResult<List<TransactionDetailDto>>> GetRecentTransactions(
            [FromQuery] int count = 10)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var transactions = await _paymentService.GetDailyTransactionsAsync(today);

                return Ok(transactions.OrderByDescending(t => t.TransactionDate).Take(count));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("stats/sales-by-payment-method")]
        [ProducesResponseType(typeof(PaymentMethodStatsDto), 200)]
        public async Task<ActionResult<PaymentMethodStatsDto>> GetSalesByPaymentMethod([FromQuery] DateTime? date = null)
        {
            // ✅ Declare before try
            var targetDate = date?.Date ?? DateTime.UtcNow.Date;

            try
            {
                var summary = await _reportService.GetDailySummaryAsync(targetDate);
                return Ok(new PaymentMethodStatsDto
                {
                    Date = targetDate,
                    CashAmount = summary.CashCollected,
                    QRISAmount = summary.QRISCollected,
                    TotalAmount = summary.TotalRevenue
                });
            }
            catch (KeyNotFoundException)
            {
                // ✅ Now 'targetDate' is in scope
                return Ok(new PaymentMethodStatsDto
                {
                    Date = targetDate,
                    CashAmount = 0,
                    QRISAmount = 0,
                    TotalAmount = 0
                });
            }
        }
    }
}
