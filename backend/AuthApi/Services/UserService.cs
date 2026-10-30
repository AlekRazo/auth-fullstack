using AuthApi.Data;
using AuthApi.DTOs;
using AuthApi.Entities;
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
                ExpiresAt = currentTime.AddDays(_config.GetValue<double>("Jwt:RefreshExpirationTime")),
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

            // Validar expiración

            // Buscar usuario poseedor del token
            var user = await _db.Users.AsNoTracking().Where(u => u.IdUser == tokenResult.IdUser).FirstOrDefaultAsync();

            if (user is null) return ("Users does not exist", "", "", null);

            // Revocar tokens existentes de ese usuario
            var tokens = await _db.RefreshTokens.Where(t => t.IdUser == tokenResult.IdUser && !t.Revoked).ToListAsync();
            tokens.ForEach(t => t.Revoked = true);
            await _db.SaveChangesAsync();

            // Generar nuevos tokens
            var accessToken = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var currentTime = DateTime.UtcNow;

            await _db.RefreshTokens.AddAsync(new RefreshToken
            {
                IdUser = user.IdUser,
                TokenHash = _jwtService.HashToken(refreshToken),
                CreatedAt = currentTime,
                ExpiresAt = currentTime.AddDays(_config.GetValue<double>("Jwt:RefreshExpirationTime")),
                Revoked = false
            });

            var result = await _db.SaveChangesAsync();

            if (result == 0) return ("Error generating refresh token.", "", "", null);

            return ("", accessToken, refreshToken, currentTime);
        }
    }
}
