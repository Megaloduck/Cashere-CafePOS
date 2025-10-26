using CafePOS.API.Models;
using CafePOS.API.Data;
using CafePOS.API.DTOs;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CafePOS.API.Services
{
    public interface ITransactionService
    {
        /// Retrieves all transactions within a specific date range.
        Task<IEnumerable<TransactionDTOs>> GetTransactionsAsync(DateTime startDate, DateTime endDate);

        /// Retrieves a specific transaction by ID.
        Task<TransactionDTOs> GetTransactionByIdAsync(int id);

        /// Deletes a transaction by ID.
        Task<bool> DeleteTransactionAsync(int id);

        /// Gets the total number of transactions within a date range.
        Task<int> GetTransactionCountAsync(DateTime startDate, DateTime endDate);
    }
    public class TransactionService : ITransactionService
    {
        private readonly CafePOSContext _context;  

        public TransactionService(CafePOSContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TransactionDTOs>> GetTransactionsAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = await _context.Transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .Include(t => t.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(i => i.MenuItem)
                .ToListAsync();

            return transactions.Select(t => new TransactionDTOs
            {
                Id = t.Id,
                OrderNumber = t.Order.OrderNumber,
                PaymentMethod = t.PaymentMethod.ToString(), 
                AmountPaid = t.AmountPaid,
                OrderTotal = t.Order.TotalAmount,
                TaxAmount = t.Order.TaxAmount,
                Status = t.Status.ToString(), 
                TransactionDate = t.TransactionDate,
                Items = t.Order.OrderItems.Select(i => new OrderItemDto
                {
                    MenuItemId = i.MenuItem.Id,
                    Name = i.MenuItem.Name,
                    Quantity = i.Quantity,
                    Price = i.MenuItem.Price
                }).ToList()
            });
        }

        public async Task<TransactionDTOs> GetTransactionByIdAsync(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(i => i.MenuItem)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");

            return new TransactionDTOs
            {
                Id = transaction.Id,
                OrderNumber = transaction.Order.OrderNumber, 
                PaymentMethod = transaction.PaymentMethod.ToString(),
                AmountPaid = transaction.AmountPaid,
                OrderTotal = transaction.Order.TotalAmount,  
                TaxAmount = transaction.Order.TaxAmount,     
                Status = transaction.Status.ToString(),
                TransactionDate = transaction.TransactionDate,
                Items = transaction.Order.OrderItems.Select(i => new OrderItemDto
                {
                    MenuItemId = i.MenuItem.Id,
                    Name = i.MenuItem.Name,
                    Quantity = i.Quantity,
                    Price = i.MenuItem.Price 
                }).ToList()
            };
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return false;

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTransactionCountAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .CountAsync(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);
        }
    }
}
