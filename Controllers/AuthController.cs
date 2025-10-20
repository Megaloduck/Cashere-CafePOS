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
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
        }

        [HttpPost("validate")]
        [Authorize]
        public async Task<ActionResult<bool>> ValidateToken()
        {
            return Ok(true);
        }
    }
}