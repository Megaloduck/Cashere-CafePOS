using CafePOS.API.Models;
using CafePOS.API.Data;
namespace CafePOS.API.DTOs
{
    public class TransactionDTOs
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string PaymentMethod { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }

        public List<OrderItemDto> Items { get; set; } = new();
    }
}
