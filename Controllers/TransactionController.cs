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
    [Authorize(Roles = "FinanceOfficer,Admin")] // Adjust roles as needed
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Returns all transactions between the specified start and end dates.
        /// Example: GET /api/transactions?startDate=2025-10-20&endDate=2025-10-26
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var transactions = await _transactionService.GetTransactionsAsync(startDate, endDate);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error fetching transactions: {ex.Message}" });
            }
        }

        /// <summary>
        /// Returns a single transaction by its ID.
        /// Example: GET /api/transactions/5
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById(int id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                return Ok(transaction);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Transaction with ID {id} not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error fetching transaction: {ex.Message}" });
            }
        }

        /// <summary>
        /// Deletes a transaction by ID.
        /// Example: DELETE /api/transactions/5
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                var deleted = await _transactionService.DeleteTransactionAsync(id);
                if (!deleted)
                    return NotFound(new { message = $"Transaction with ID {id} not found." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error deleting transaction: {ex.Message}" });
            }
        }

        /// <summary>
        /// Returns total transaction count for a given period.
        /// Example: GET /api/transactions/count?startDate=2025-10-20&endDate=2025-10-26
        /// </summary>
        [HttpGet("count")]
        public async Task<IActionResult> GetTransactionCount([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var count = await _transactionService.GetTransactionCountAsync(startDate, endDate);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error fetching transaction count: {ex.Message}" });
            }
        }
    }
}
