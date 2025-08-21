using InforceTestTask.DTOs.Requests;
using InforceTestTask.DTOs.Responses;
using InforceTestTask.Models;
using InforceTestTask.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InforceTestTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            var result = await authService.RegisterAsync(request);

            if (result == null)
                return BadRequest("Try again");

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await authService.LoginAsync(request);
             
            if (result == null)
                return BadRequest("Login or Password is wrong.");

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await authService.RefreshTokenAsync(request);
            if (result == null || result.AccessToken == null || result.RefreshToken == null)
            {
                return BadRequest("Invalid refresh token.");
            }
                
            return Ok(result);
        }

        [Authorize]
        [HttpPost("get-user-id")]
        public async Task<ActionResult<UserIdRoleResponse>> GetUserId([FromBody] UserIdRoleRequest request)
        {
            var result = await authService.GetUserIdAsync(request);
            if (result == null)
                return NotFound("User not found.");
            return Ok(result);
        }
    }
}
