using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CafePOS.API.DTOs;
using CafePOS.API.Services;
using CafePOS.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CafePOS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<List<MenuCategoryDto>>> GetCategories()
        {
            try
            {
                var categories = await _menuService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("category/{categoryId}/items")]
        public async Task<ActionResult<List<MenuItemDto>>> GetItemsByCategory(int categoryId)
        {
            try
            {
                var items = await _menuService.GetItemsByCategoryAsync(categoryId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("item/{itemId}")]
        public async Task<ActionResult<MenuItemDto>> GetMenuItem(int itemId)
        {
            try
            {
                var item = await _menuService.GetMenuItemAsync(itemId);
                return Ok(item);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("tax-settings")]
        public async Task<ActionResult<TaxSettingsDto>> GetTaxSettings()
        {
            try
            {
                var settings = await _menuService.GetTaxSettingsAsync();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
