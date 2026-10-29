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
            var (errorMessage, token, refreshToken) = await _userService.Login(req);

            if (errorMessage != "") return Conflict(errorMessage);

            var expirationMinutes = _config.GetValue<double>("Jwt:ExpirationTime");
            //EXTRA: REFRESH
            var expirationDays = _config.GetValue<double>("Jwt:RefreshExpirationTime");
            //

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, //False si no se usa HTTPS
                SameSite = SameSiteMode.None, //None
                Expires = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
            });

            //EXTRA: REFRESH
            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, //False si no se usa HTTPS
                SameSite = SameSiteMode.None, //None
                Expires = DateTimeOffset.UtcNow.AddDays(expirationDays)
            });
            //

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
        public IActionResult Logout()
        {
            //Almacenar el nombre de la cookie en appsettings
            Response.Cookies.Delete("access_token");
            return Ok();
        }

        //EXTRA : REFRESH
        [HttpPost("refresh")]
        public IActionResult Refresh()
        {
            HttpContext.Request.Cookies.TryGetValue("access_token", out var accessToken);
            HttpContext.Request.Cookies.TryGetValue("refresh_token", out var refreshToken);

            return Ok();
        }
    }
}
