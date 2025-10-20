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
    [Authorize(Roles = "Cashier,Admin")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IReportService _reportService;
        private readonly IHubContext<SalesHub> _hubContext;

        public PaymentController(IPaymentService paymentService, IReportService reportService, IHubContext<SalesHub> hubContext)
        {
            _paymentService = paymentService;
            _reportService = reportService;
            _hubContext = hubContext;
        }

        [HttpPost("process")]
        public async Task<ActionResult<PaymentResponse>> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(request);

                // Notify finance dashboard in real-time
                await _hubContext.Clients.All.SendAsync("SaleCompleted", payment.OrderNumber, payment.OrderTotal);

                // Update daily summary
                await _reportService.UpdateDailySummaryAsync();

                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{transactionId}")]
        public async Task<ActionResult<PaymentResponse>> GetPayment(int transactionId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentAsync(transactionId);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("daily/{date}")]
        public async Task<ActionResult<List<TransactionDetailDto>>> GetDailyTransactions(string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsedDate))
                    return BadRequest(new { message = "Invalid date format" });

                var transactions = await _paymentService.GetDailyTransactionsAsync(parsedDate);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
