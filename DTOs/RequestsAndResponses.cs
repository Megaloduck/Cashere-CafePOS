using System;
using CafePOS.API.DTOs;
using CafePOS.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace CafePOS.API.DTOs
{
    // Authentication DTOs
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    // Menu DTOs
    public class MenuCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public List<MenuItemDto> Items { get; set; } = new();
    }

    public class MenuItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsTaxable { get; set; }
        public int DisplayOrder { get; set; }
    }

    // Order DTOs
    public class CreateOrderItemRequest
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderItemDto
    {
        public int MenuItemId { get; set; }     
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? TaxRate { get; set; }
    }

    public class CreateOrderRequest
    {
        public List<CreateOrderItemRequest> Items { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Status { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

    // Payment DTOs
    public class ProcessPaymentRequest
    {
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; } // "Cash" or "QRIS"
        public decimal AmountPaid { get; set; }
    }

    public class PaymentResponse
    {
        public int TransactionId { get; set; }
        public string OrderNumber { get; set; }
        public string PaymentMethod { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public string Status { get; set; }
        public string QRCodeData { get; set; } // For QRIS
        public string ReferenceNumber { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    // Report DTOs
    public class DailySummaryDto
    {
        public DateTime SummaryDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalTax { get; set; }
        public decimal CashCollected { get; set; }
        public decimal QRISCollected { get; set; }
        public int TotalItemsSold { get; set; }
    }

    public class TransactionDetailDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string PaymentMethod { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

    // Tax Settings DTOs
    public class TaxSettingsDto
    {
        public int Id { get; set; }
        public decimal DefaultTaxRate { get; set; }
        public string TaxName { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class UpdateTaxSettingsRequest
    {
        public decimal DefaultTaxRate { get; set; }
        public string TaxName { get; set; }
        public bool IsEnabled { get; set; }
    }
}