using CafePOS.API.Models;
using CafePOS.API.Data;
using System;
using System.Collections.Generic;

namespace CafePOS.API.Models
{
    // User & Authentication
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public enum UserRole
    {
        Cashier = 1,
        FinanceOfficer = 2,
        Admin = 3
    }

    // Menu Management
    public class MenuCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }

    public class MenuItem
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsTaxable { get; set; }
        public decimal? CustomTaxRate { get; set; } // Override default tax rate if needed
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public MenuCategory Category { get; set; }
    }

    // Tax Settings
    public class TaxSettings
    {
        public int Id { get; set; }
        public decimal DefaultTaxRate { get; set; } // e.g., 0.10 for 10%
        public string TaxName { get; set; } // e.g., "PPN", "VAT"
        public bool IsEnabled { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Order Management
    public class Order
    {
        public int Id { get; set; }
        public int CashierId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; } // Optional discount
        public OrderStatus Status { get; set; }
        public string OrderNumber { get; set; } // Unique receipt number
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User Cashier { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Transaction Transaction { get; set; }
    }

    public enum OrderStatus
    {
        Pending = 1,
        Completed = 2,
        Cancelled = 3
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsTaxable { get; set; }
        public decimal? TaxRate { get; set; }

        public Order Order { get; set; }
        public MenuItem MenuItem { get; set; }
    }

    // Payment & Transactions
    public class Transaction
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public TransactionStatus Status { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal ChangeAmount { get; set; } // For cash payments
        public string ReferenceNumber { get; set; } // For QRIS tracking
        public string QRCodeData { get; set; } // QRIS QR code string
        public DateTime TransactionDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Order Order { get; set; }
    }

    public enum PaymentMethod
    {
        Cash = 1,
        QRIS = 2
    }

    public enum TransactionStatus
    {
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5
    }

    // Daily Report/Summary
    public class DailySummary
    {
        public int Id { get; set; }
        public DateTime SummaryDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalTax { get; set; }
        public decimal CashCollected { get; set; }
        public decimal QRISCollected { get; set; }
        public int TotalItemsSold { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}