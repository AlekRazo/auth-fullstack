using AuthApi.Data;
using AuthApi.DTOs;
using AuthApi.Entities;
using Azure;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Services
{
    public class UserService (AuthDbContext _db, PasswordService _passwordService, JwtService _jwtService, IConfiguration _config)
    {
        public async Task<string> Register(RegisterRequest req)
        {
            var emailExists = _db.Users.Any(u => u.Email.Equals(req.email));

            if (emailExists) return "Email already exists";

            var user = new User
            {
                Name = req.name,
                Email = req.email,
                PasswordHash = _passwordService.Hash(req.password),
                IdRole = req.idRole
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return "";
        }

        public async Task<(string errorMessage, string token, string refreshToken, DateTime? currentDateTime)> Login(LoginRequest req)
        {
            var user = await _db.Users.AsNoTracking().Where(u => u.Email.Equals(req.email)).Include(r => r.IdRoleNavigation).FirstOrDefaultAsync();

            if (user is null) return ("Incorrect Credentials", "", "", null);

            var passwordValid = _passwordService.Verify(req.password, user.PasswordHash);

            if (!passwordValid) return ("Incorrect credentials", "", "", null);

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var currentTime = DateTime.UtcNow;

            await _db.RefreshTokens.AddAsync(new RefreshToken
            {
                IdUser = user.IdUser,
                TokenHash = _jwtService.HashToken(refreshToken),
                CreatedAt = currentTime,
                //ExpiresAt = currentTime.AddDays(_config.GetValue<double>("Jwt:RefreshExpirationTime")),
                ExpiresAt = currentTime.AddMinutes(5.0),
                Revoked = false
            });

            var result = await _db.SaveChangesAsync();

            if (result == 0) return ("Error generating refresh token.", "", "", null);

            return ("", token, refreshToken, currentTime);
        }

        public async Task<GetUserResponse?> Me(int idUser)
        {
            var userFound = await _db.Users.AsNoTracking().Where(u => u.IdUser == idUser).Include(r => r.IdRoleNavigation).ThenInclude(rm => rm.RoleMenus).ThenInclude(m => m.IdMenuNavigation).FirstOrDefaultAsync();

            if (userFound is null) return null;

            var userResponse = new GetUserResponse
            (
                name: userFound.Name,
                email: userFound.Email,
                role: userFound.IdRoleNavigation.Name,
                menus: userFound.IdRoleNavigation.RoleMenus.Select(rm => new GetMenuResponse(name: rm.IdMenuNavigation.Name, link: rm.IdMenuNavigation.Link))
            );

            return userResponse;
        }

        // EXTRA: REFRESH
        public async Task<(string errorMessage, string token, string refreshToken, DateTime? currentDateTime)> Refresh(string token)
        {
            // Validar viejo token
            if (string.IsNullOrEmpty(token)) return ("There is no token", "", "", null);

            // Verificar que exista el viejo token
            var hashToken = _jwtService.HashToken(token);
            var tokenResult = await _db.RefreshTokens.AsNoTracking().Where(r => r.TokenHash == hashToken).FirstOrDefaultAsync();

            if (tokenResult is null) return ("Invalid refresh token", "", "", null);

            // Terminar: Revocar token si se reutiliza
            if (tokenResult.Revoked)
            {
                var allTokens = await _db.RefreshTokens.Where(t => t.IdUser == tokenResult.IdUser && !t.Revoked).ToListAsync();
                allTokens.ForEach(t => t.Revoked = true);
                await _db.SaveChangesAsync();

                return ("Invalid refresh token", "", "", null);
            }

            // Validar expiración
            if (tokenResult.ExpiresAt < DateTime.UtcNow) return ("Expired token", "", "", null);

            // Buscar usuario poseedor del token
            var user = await _db.Users.AsNoTracking().Where(u => u.IdUser == tokenResult.IdUser).Include(r => r.IdRoleNavigation).ThenInclude(rm => rm.RoleMenus).ThenInclude(m => m.IdMenuNavigation).FirstOrDefaultAsync();

            if (user is null) return ("Users does not exist", "", "", null);

            // Revocar token
            tokenResult.Revoked = true;

            // Generar nuevos tokens
            var accessToken = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var currentTime = DateTime.UtcNow;

            await _db.RefreshTokens.AddAsync(new RefreshToken
            {
                IdUser = user.IdUser,
                TokenHash = _jwtService.HashToken(refreshToken),
                CreatedAt = currentTime,
                //ExpiresAt = currentTime.AddDays(_config.GetValue<double>("Jwt:RefreshExpirationTime")),
                ExpiresAt = currentTime.AddMinutes(5.0),
                Revoked = false
            });

            var result = await _db.SaveChangesAsync();

            return ("", accessToken, refreshToken, currentTime);
        }

        public async Task Logout(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) return;

            var hashedToken = _jwtService.HashToken(refreshToken);
            var tokenResult = await _db.RefreshTokens.AsNoTracking().Where(r => r.TokenHash == hashedToken).FirstOrDefaultAsync();
            var user = await _db.Users.AsNoTracking().Where(u => u.IdUser == tokenResult!.IdUser).FirstOrDefaultAsync();
            var allTokens = await _db.RefreshTokens.Where(t => t.IdUser == user!.IdUser).ToListAsync();
            allTokens.ForEach(t => t.Revoked = true);
            await _db.SaveChangesAsync();
        }

        public CookieOptions CreateCookie(DateTime expires, string path = "/") => new CookieOptions()
        {
            HttpOnly = true,
            Secure = true, //False si no se usa HTTPS
            SameSite = SameSiteMode.None,
            Expires = expires,
            Path = path
        };
    }
}
