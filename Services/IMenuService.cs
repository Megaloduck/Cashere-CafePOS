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
    // ============ MENU SERVICE ============
    public interface IMenuService
    {
        Task<List<MenuCategoryDto>> GetAllCategoriesAsync();
        Task<List<MenuItemDto>> GetItemsByCategoryAsync(int categoryId);
        Task<MenuItemDto> GetMenuItemAsync(int itemId);
        Task<TaxSettingsDto> GetTaxSettingsAsync();
    }

    public class MenuService : IMenuService
    {
        private readonly CafePOSContext _context;

        public MenuService(CafePOSContext context)
        {
            _context = context;
        }

        public async Task<List<MenuCategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.MenuCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new MenuCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder,
                    Items = c.MenuItems
                        .Where(i => i.IsActive)
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => new MenuItemDto
                        {
                            Id = i.Id,
                            Name = i.Name,
                            Description = i.Description,
                            Price = i.Price,
                            IsTaxable = i.IsTaxable,
                            DisplayOrder = i.DisplayOrder
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<List<MenuItemDto>> GetItemsByCategoryAsync(int categoryId)
        {
            return await _context.MenuItems
                .Where(i => i.CategoryId == categoryId && i.IsActive)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new MenuItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    IsTaxable = i.IsTaxable,
                    DisplayOrder = i.DisplayOrder
                })
                .ToListAsync();
        }

        public async Task<MenuItemDto> GetMenuItemAsync(int itemId)
        {
            var item = await _context.MenuItems.FindAsync(itemId);
            if (item == null)
                throw new KeyNotFoundException($"Menu item {itemId} not found");

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

        public async Task<TaxSettingsDto> GetTaxSettingsAsync()
        {
            var taxSettings = await _context.TaxSettings.FirstAsync();
            return new TaxSettingsDto
            {
                Id = taxSettings.Id,
                DefaultTaxRate = taxSettings.DefaultTaxRate,
                TaxName = taxSettings.TaxName,
                IsEnabled = taxSettings.IsEnabled
            };
        }
    }
}
