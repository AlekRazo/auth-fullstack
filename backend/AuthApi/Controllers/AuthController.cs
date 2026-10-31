using AuthApi.DTOs;
using AuthApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(UserService _userService, IConfiguration _config) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest req)
        {
            var errorMessage = await _userService.Register(req);

            if (errorMessage != string.Empty) return Conflict(errorMessage);

            return Created();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var (errorMessage, token, refreshToken, currentDateTime) = await _userService.Login(req);

            if (errorMessage != "") return Conflict(errorMessage);

            var expirationMinutes = _config.GetValue<double>("Jwt:ExpirationTime");
            // EXTRA: REFRESH
            var expirationDays = _config.GetValue<double>("Jwt:RefreshExpirationTime");
            Response.Cookies.Append("access_token", token, _userService.CreateCookie(currentDateTime!.Value.AddMinutes(1)));
            // EXTRA: REFRESH
            Response.Cookies.Append("refresh_token", refreshToken, _userService.CreateCookie(currentDateTime!.Value.AddMinutes(5), "/api/Auth"));

            return Ok();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userId, out var id)) return Unauthorized();

            var user = await _userService.Me(id);

            if (user is null) Unauthorized();

            return Ok(user);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            //Almacenar el nombre de la cookie en appsettings
            HttpContext.Request.Cookies.TryGetValue("refresh_token", out var token);
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            await _userService.Logout(token!);
            return Ok();
        }

        // EXTRA : REFRESH
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            HttpContext.Request.Cookies.TryGetValue("refresh_token", out var token);
            var (errorMessage, accessToken, refreshToken, currentDateTime) = await _userService.Refresh(token!);

            if (!string.IsNullOrEmpty(errorMessage)) return Unauthorized();

            var expirationMinutes = _config.GetValue<double>("Jwt:ExpirationTime"); // Optional: Use null-check and throw InvalidOperationException in case value is null
            // EXTRA: REFRESH
            var expirationDays = _config.GetValue<double>("Jwt:RefreshExpirationTime"); // Optional: Use null-check and throw InvalidOperationException in case value is null
            Response.Cookies.Append("access_token", accessToken, _userService.CreateCookie(currentDateTime!.Value.AddMinutes(1)));
            // EXTRA: REFRESH
            Response.Cookies.Append("refresh_token", refreshToken, _userService.CreateCookie(currentDateTime!.Value.AddMinutes(5), "/api/Auth"));

            return Ok();
        }
    }
}
