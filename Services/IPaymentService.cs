using CafePOS.API.DTOs;
using CafePOS.API.Models;
using CafePOS.API.Data;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CafePOS.API.Services
{
    // ============ PAYMENT SERVICE ============
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request);
        Task<PaymentResponse> GetPaymentAsync(int transactionId);
        Task<List<TransactionDetailDto>> GetDailyTransactionsAsync(DateTime date);
    }

    public class PaymentService : IPaymentService
    {
        private readonly CafePOSContext _context;

        public PaymentService(CafePOSContext context)
        {
            _context = context;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
                throw new KeyNotFoundException($"Order {request.OrderId} not found");

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException("Order is not pending");

            PaymentMethod paymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod);

            var transaction = new Transaction
            {
                OrderId = order.Id,
                PaymentMethod = paymentMethod,
                Status = TransactionStatus.Processing,
                AmountPaid = request.AmountPaid,
                ChangeAmount = request.AmountPaid - order.TotalAmount,
                ReferenceNumber = GenerateReferenceNumber(),
                TransactionDate = DateTime.UtcNow
            };

            // For QRIS, generate QR code (integrate with QRIS provider)
            if (paymentMethod == PaymentMethod.QRIS)
            {
                transaction.QRCodeData = GenerateQRISData(order.TotalAmount, transaction.ReferenceNumber);
                transaction.Status = TransactionStatus.Pending; // Waiting for user to scan
            }
            else
            {
                transaction.Status = TransactionStatus.Completed;
                order.Status = OrderStatus.Completed;
            }

            _context.Transactions.Add(transaction);
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapToPaymentResponse(transaction, order);
        }

        public async Task<PaymentResponse> GetPaymentAsync(int transactionId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Order)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null)
                throw new KeyNotFoundException($"Transaction {transactionId} not found");

            return MapToPaymentResponse(transaction, transaction.Order);
        }

        public async Task<List<TransactionDetailDto>> GetDailyTransactionsAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = date.Date.AddDays(1);

            return await _context.Transactions
                .Include(t => t.Order)
                .ThenInclude(o => o.OrderItems)
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate < endDate)
                .Select(t => new TransactionDetailDto
                {
                    Id = t.Id,
                    OrderNumber = t.Order.OrderNumber,
                    PaymentMethod = t.PaymentMethod.ToString(),
                    AmountPaid = t.AmountPaid,
                    OrderTotal = t.Order.TotalAmount,
                    TaxAmount = t.Order.TaxAmount,
                    Status = t.Status.ToString(),
                    TransactionDate = t.TransactionDate,
                    Items = t.Order.OrderItems.Select(oi => new OrderItemDto
                    {
                        ItemName = oi.ItemName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        SubtotalAmount = oi.SubtotalAmount,
                        TaxAmount = oi.TaxAmount,
                        TotalAmount = oi.TotalAmount
                    }).ToList()
                })
                .ToListAsync();
        }

        private string GenerateReferenceNumber()
        {
            return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }

        private string GenerateQRISData(decimal amount, string reference)
        {
            // This is a placeholder. Integrate with your QRIS provider (DOKU, BNI, etc.)
            // QRIS providers will give you specific format for generating QR codes
            return $"00020126360014ID.CO.BRI.BRIVA0215{amount:F2}520441555802ID5913CAFE2215020000{reference}63041D31";
        }

        private PaymentResponse MapToPaymentResponse(Transaction transaction, Order order)
        {
            return new PaymentResponse
            {
                TransactionId = transaction.Id,
                OrderNumber = order.OrderNumber,
                PaymentMethod = transaction.PaymentMethod.ToString(),
                AmountPaid = transaction.AmountPaid,
                ChangeAmount = transaction.ChangeAmount,
                OrderTotal = order.TotalAmount,
                TaxAmount = order.TaxAmount,
                Status = transaction.Status.ToString(),
                QRCodeData = transaction.QRCodeData,
                ReferenceNumber = transaction.ReferenceNumber,
                TransactionDate = transaction.TransactionDate
            };
        }
    }
}
