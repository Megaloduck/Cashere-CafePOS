using CafePOS.API.DTOs;
using CafePOS.Core.Models;
using CafePOS.Data;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CafePOS.API.Services
{
    // ============ ORDER SERVICE ============
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, int cashierId);
        Task<OrderDto> GetOrderAsync(int orderId);
        Task<OrderDto> UpdateOrderAsync(int orderId, CreateOrderRequest request);
        Task CancelOrderAsync(int orderId);
        string GenerateOrderNumber();
    }

    public class OrderService : IOrderService
    {
        private readonly CafePOSContext _context;
        private readonly IMenuService _menuService;

        public OrderService(CafePOSContext context, IMenuService menuService)
        {
            _context = context;
            _menuService = menuService;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, int cashierId)
        {
            var taxSettings = await _menuService.GetTaxSettingsAsync();
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                CashierId = cashierId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                DiscountAmount = request.DiscountAmount
            };

            decimal subtotal = 0;
            decimal totalTax = 0;

            foreach (var itemRequest in request.Items)
            {
                var menuItem = await _context.MenuItems.FindAsync(itemRequest.MenuItemId);
                if (menuItem == null)
                    throw new KeyNotFoundException($"Menu item {itemRequest.MenuItemId} not found");

                decimal itemSubtotal = menuItem.Price * itemRequest.Quantity;
                decimal itemTax = 0;
                decimal taxRate = taxSettings.DefaultTaxRate;

                if (menuItem.IsTaxable)
                {
                    if (menuItem.CustomTaxRate.HasValue)
                        taxRate = menuItem.CustomTaxRate.Value;

                    itemTax = itemSubtotal * taxRate;
                }

                var orderItem = new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    ItemName = menuItem.Name,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = menuItem.Price,
                    SubtotalAmount = itemSubtotal,
                    TaxAmount = itemTax,
                    TotalAmount = itemSubtotal + itemTax,
                    IsTaxable = menuItem.IsTaxable,
                    TaxRate = menuItem.IsTaxable ? taxRate : null
                };

                order.OrderItems.Add(orderItem);
                subtotal += itemSubtotal;
                totalTax += itemTax;
            }

            order.SubtotalAmount = subtotal;
            order.TaxAmount = totalTax;
            order.TotalAmount = subtotal + totalTax - request.DiscountAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return MapToOrderDto(order);
        }

        public async Task<OrderDto> GetOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException($"Order {orderId} not found");

            return MapToOrderDto(order);
        }

        public async Task<OrderDto> UpdateOrderAsync(int orderId, CreateOrderRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException($"Order {orderId} not found");

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException("Can only update pending orders");

            _context.OrderItems.RemoveRange(order.OrderItems);
            order.OrderItems.Clear();

            var taxSettings = await _menuService.GetTaxSettingsAsync();
            decimal subtotal = 0;
            decimal totalTax = 0;

            foreach (var itemRequest in request.Items)
            {
                var menuItem = await _context.MenuItems.FindAsync(itemRequest.MenuItemId);
                if (menuItem == null)
                    throw new KeyNotFoundException($"Menu item {itemRequest.MenuItemId} not found");

                decimal itemSubtotal = menuItem.Price * itemRequest.Quantity;
                decimal itemTax = 0;
                decimal taxRate = taxSettings.DefaultTaxRate;

                if (menuItem.IsTaxable)
                {
                    if (menuItem.CustomTaxRate.HasValue)
                        taxRate = menuItem.CustomTaxRate.Value;

                    itemTax = itemSubtotal * taxRate;
                }

                var orderItem = new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    ItemName = menuItem.Name,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = menuItem.Price,
                    SubtotalAmount = itemSubtotal,
                    TaxAmount = itemTax,
                    TotalAmount = itemSubtotal + itemTax,
                    IsTaxable = menuItem.IsTaxable,
                    TaxRate = menuItem.IsTaxable ? taxRate : null
                };

                order.OrderItems.Add(orderItem);
                subtotal += itemSubtotal;
                totalTax += itemTax;
            }

            order.SubtotalAmount = subtotal;
            order.TaxAmount = totalTax;
            order.TotalAmount = subtotal + totalTax - request.DiscountAmount;
            order.DiscountAmount = request.DiscountAmount;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToOrderDto(order);
        }

        public async Task CancelOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order {orderId} not found");

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                SubtotalAmount = order.SubtotalAmount,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                Status = order.Status.ToString(),
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ItemName = oi.ItemName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    SubtotalAmount = oi.SubtotalAmount,
                    TaxAmount = oi.TaxAmount,
                    TotalAmount = oi.TotalAmount,
                    TaxRate = oi.TaxRate
                }).ToList()
            };
        }
    }
}
