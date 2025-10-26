using Microsoft.EntityFrameworkCore;
using CafePOS.API.Models;
using CafePOS.API.Data;
using CafePOS.API.DTOs;

namespace CafePOS.API.Services
{
    public interface IAdminMenuService
    {
        Task<MenuCategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
        Task<MenuCategoryDto> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
        Task DeleteCategoryAsync(int id);

        Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemRequest request);
        Task<MenuItemDto> UpdateMenuItemAsync(int id, UpdateMenuItemRequest request);
        Task DeleteMenuItemAsync(int id);

        Task<TaxSettingsDto> UpdateTaxSettingsAsync(UpdateTaxSettingsRequest request);
    }

    public class AdminMenuService : IAdminMenuService
    {
        private readonly CafePOSContext _context;

        public AdminMenuService(CafePOSContext context)
        {
            _context = context;
        }

        // ============ CATEGORIES ============
        public async Task<MenuCategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
        {
            var category = new MenuCategory
            {
                Name = request.Name,
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.MenuCategories.Add(category);
            await _context.SaveChangesAsync();

            return new MenuCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder
            };
        }

        public async Task<MenuCategoryDto> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            var category = await _context.MenuCategories.FindAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category {id} not found");

            category.Name = request.Name;
            category.Description = request.Description;
            category.DisplayOrder = request.DisplayOrder;
            category.IsActive = request.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new MenuCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder
            };
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.MenuCategories.FindAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category {id} not found");

            // Soft delete - just mark as inactive
            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // ============ MENU ITEMS ============
        public async Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemRequest request)
        {
            var item = new MenuItem
            {
                CategoryId = request.CategoryId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                IsTaxable = request.IsTaxable,
                CustomTaxRate = request.CustomTaxRate,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            return new MenuItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                IsTaxable = item.IsTaxable,
                DisplayOrder = item.DisplayOrder
            };
        }

        public async Task<MenuItemDto> UpdateMenuItemAsync(int id, UpdateMenuItemRequest request)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
                throw new KeyNotFoundException($"Menu item {id} not found");

            item.CategoryId = request.CategoryId;
            item.Name = request.Name;
            item.Description = request.Description;
            item.Price = request.Price;
            item.IsTaxable = request.IsTaxable;
            item.CustomTaxRate = request.CustomTaxRate;
            item.DisplayOrder = request.DisplayOrder;
            item.IsActive = request.IsActive;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new MenuItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                IsTaxable = item.IsTaxable,
                DisplayOrder = item.DisplayOrder
            };
        }

        public async Task DeleteMenuItemAsync(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
                throw new KeyNotFoundException($"Menu item {id} not found");

            // Soft delete
            item.IsActive = false;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // ============ TAX SETTINGS ============
        public async Task<TaxSettingsDto> UpdateTaxSettingsAsync(UpdateTaxSettingsRequest request)
        {
            var settings = await _context.TaxSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new TaxSettings
                {
                    DefaultTaxRate = request.DefaultTaxRate,
                    TaxName = request.TaxName,
                    IsEnabled = request.IsEnabled,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.TaxSettings.Add(settings);
            }
            else
            {
                settings.DefaultTaxRate = request.DefaultTaxRate;
                settings.TaxName = request.TaxName;
                settings.IsEnabled = request.IsEnabled;
                settings.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new TaxSettingsDto
            {
                Id = settings.Id,
                DefaultTaxRate = settings.DefaultTaxRate,
                TaxName = settings.TaxName,
                IsEnabled = settings.IsEnabled
            };
        }
    }
}
