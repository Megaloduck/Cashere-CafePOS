using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CafePOS.API.DTOs;
using CafePOS.API.Services;

namespace CafePOS.API.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MenuManagementController : ControllerBase
    {
        private readonly IAdminMenuService _menuService;

        public MenuManagementController(IAdminMenuService menuService)
        {
            _menuService = menuService;
        }

        // ============ CATEGORIES ============
        [HttpPost("category")]
        public async Task<ActionResult<MenuCategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                var category = await _menuService.CreateCategoryAsync(request);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("category/{id}")]
        public async Task<ActionResult<MenuCategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var category = await _menuService.UpdateCategoryAsync(id, request);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("category/{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                await _menuService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ============ MENU ITEMS ============
        [HttpPost("item")]
        public async Task<ActionResult<MenuItemDto>> CreateMenuItem([FromBody] CreateMenuItemRequest request)
        {
            try
            {
                var item = await _menuService.CreateMenuItemAsync(request);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("item/{id}")]
        public async Task<ActionResult<MenuItemDto>> UpdateMenuItem(int id, [FromBody] UpdateMenuItemRequest request)
        {
            try
            {
                var item = await _menuService.UpdateMenuItemAsync(id, request);
                return Ok(item);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("item/{id}")]
        public async Task<ActionResult> DeleteMenuItem(int id)
        {
            try
            {
                await _menuService.DeleteMenuItemAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ============ TAX SETTINGS ============
        [HttpPut("tax-settings")]
        public async Task<ActionResult<TaxSettingsDto>> UpdateTaxSettings([FromBody] UpdateTaxSettingsRequest request)
        {
            try
            {
                var settings = await _menuService.UpdateTaxSettingsAsync(request);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}