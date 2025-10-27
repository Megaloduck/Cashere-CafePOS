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
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("API is running");
    }
}
