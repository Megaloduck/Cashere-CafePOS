using System;
using CafePOS.API.DTOs;
using CafePOS.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace CafePOS.API.DTOs
{
    // Category DTOs
    public class CreateCategoryRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpdateCategoryRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    // Menu Item DTOs
    public class CreateMenuItemRequest
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsTaxable { get; set; }
        public decimal? CustomTaxRate { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpdateMenuItemRequest
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsTaxable { get; set; }
        public decimal? CustomTaxRate { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    // User DTOs
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // "Cashier", "FinanceOfficer", "Admin"
    }

    public class UpdateUserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; }
    }

    // Dashboard DTOs
    public class PaymentMethodStatsDto
    {
        public DateTime Date { get; set; }
        public decimal CashAmount { get; set; }
        public decimal QRISAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
