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
    // ============ REPORT SERVICE ============
    public interface IReportService
    {
        Task<DailySummaryDto> GetDailySummaryAsync(DateTime date);
        Task UpdateDailySummaryAsync();
        Task<IEnumerable<TopSellingItemDTOs>> GetTopSellingItemsAsync(DateTime startDate, DateTime endDate, int count = 10);

    }

    public class ReportService : IReportService
    {
        private readonly CafePOSContext _context;

        public ReportService(CafePOSContext context)
        {
            _context = context;
        }

        public async Task<DailySummaryDto> GetDailySummaryAsync(DateTime date)
        {
            var summary = await _context.DailySummaries
                .FirstOrDefaultAsync(s => s.SummaryDate == date.Date);

            if (summary == null)
                throw new KeyNotFoundException($"No summary found for {date:yyyy-MM-dd}");

            return new DailySummaryDto
            {
                SummaryDate = summary.SummaryDate,
                TotalTransactions = summary.TotalTransactions,
                TotalRevenue = summary.TotalRevenue,
                TotalTax = summary.TotalTax,
                CashCollected = summary.CashCollected,
                QRISCollected = summary.QRISCollected,
                TotalItemsSold = summary.TotalItemsSold
            };
        }

        public async Task UpdateDailySummaryAsync()
        {
            var today = DateTime.UtcNow.Date;
            var transactions = await _context.Transactions
                .Include(t => t.Order)
                .ThenInclude(o => o.OrderItems)
                .Where(t => t.TransactionDate.Date == today && t.Status == TransactionStatus.Completed)
                .ToListAsync();

            var summary = await _context.DailySummaries
                .FirstOrDefaultAsync(s => s.SummaryDate == today) ?? new DailySummary { SummaryDate = today };

            summary.TotalTransactions = transactions.Count;
            summary.TotalRevenue = transactions.Sum(t => t.Order.TotalAmount);
            summary.TotalTax = transactions.Sum(t => t.Order.TaxAmount);
            summary.CashCollected = transactions.Where(t => t.PaymentMethod == PaymentMethod.Cash).Sum(t => t.AmountPaid);
            summary.QRISCollected = transactions.Where(t => t.PaymentMethod == PaymentMethod.QRIS).Sum(t => t.AmountPaid);
            summary.TotalItemsSold = transactions.SelectMany(t => t.Order.OrderItems).Sum(oi => oi.Quantity);
            summary.UpdatedAt = DateTime.UtcNow;

            if (summary.Id == 0)
                _context.DailySummaries.Add(summary);

            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<TopSellingItemDTOs>> GetTopSellingItemsAsync(DateTime startDate, DateTime endDate, int count = 10)
        {
            var transactions = await _context.Transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .Include(t => t.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.MenuItem)
                        .ThenInclude(m => m.Category)
                .ToListAsync();

            var topItems = transactions
                .SelectMany(t => t.Order.OrderItems)
                .GroupBy(oi => new
                {
                    ItemId = oi.MenuItem.Id,
                    ItemName = oi.MenuItem.Name,
                    CategoryName = oi.MenuItem.Category.Name
                })
                .Select(g => new TopSellingItemDTOs
                {
                    Name = g.Key.ItemName,
                    Category = g.Key.CategoryName,
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.UnitPrice * x.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(count)
                .ToList();

            return topItems;
        }


    }
}
