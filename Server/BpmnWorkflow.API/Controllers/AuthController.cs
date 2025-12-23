using Microsoft.AspNetCore.Mvc;
using BpmnWorkflow.Application.Interfaces;
using BpmnWorkflow.Client.Models;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace BpmnWorkflow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.Success)
            {
                if (result.Message.StartsWith("SERVER_ERROR"))
                {
                    return StatusCode(500, new { error = "Internal Server Error", details = result.Message });
                }
                return Unauthorized(new { error = "Unauthorized", message = result.Message });
            }
            return Ok(result);
        }
    }
}
