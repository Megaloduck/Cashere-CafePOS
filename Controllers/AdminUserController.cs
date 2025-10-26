using CafePOS.API.DTOs;
using CafePOS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafePOS.API.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : ControllerBase
    {
        private readonly IAdminUserService _userService;

        public UserManagementController(IAdminUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetUserAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("users")]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _userService.CreateUserAsync(request);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("users/{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, request);
                return Ok(user);
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

        [HttpDelete("users/{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                // Get currently logged-in user's ID from JWT or session
                var performedById = int.Parse(User.FindFirst("id")?.Value ?? "0");

                await _userService.DeleteUserAsync(id, performedById);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("users/{id}/reset-password")]
        public async Task<ActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
        {
            try
            {
                await _userService.ResetPasswordAsync(id, request.NewPassword);
                return Ok(new { message = "Password reset successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
