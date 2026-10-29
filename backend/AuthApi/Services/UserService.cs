using AuthApi.Data;
using AuthApi.DTOs;
using AuthApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Services
{
    public class UserService (AuthDbContext _db, PasswordService _passwordService, JwtService _jwtService)
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

        public async Task<(string errorMessage, string token, string refreshToken)> Login(LoginRequest req)
        {
            var user = await _db.Users.AsNoTracking().Where(u => u.Email.Equals(req.email)).Include(r => r.IdRoleNavigation).FirstOrDefaultAsync();

            if (user is null) return ("Incorrect Credentials", "", "");

            var passwordValid = _passwordService.Verify(req.password, user.PasswordHash);

            if (!passwordValid) return ("Incorrect credentials", "", "");

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return ("", token, refreshToken);
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
        public async Task<(string errorMessage, string token, string refreshToken)> Refresh(string token)
        {
            var tokenResult = await _db.RefreshTokens.AsNoTracking().Where(r => r.TokenHash == token).FirstOrDefaultAsync();

            if (user is null) return ("Incorrect Credentials", "", "");

            var passwordValid = _passwordService.Verify(req.password, user.PasswordHash);

            if (!passwordValid) return ("Incorrect credentials", "", "");

            var accessToken = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return ("", accessToken, refreshToken);
        }
    }
}
