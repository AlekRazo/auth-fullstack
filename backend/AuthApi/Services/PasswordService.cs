using Microsoft.AspNetCore.Identity;

namespace AuthApi.Services
{
    public class PasswordService
    {
        private readonly PasswordHasher<string> _hasher = new PasswordHasher<string>();

        public string Hash(string password)
        {
            return _hasher.HashPassword(null!, password);
        }

        public bool Verify(string password, string hashedPassword)
        {
            var result = _hasher.VerifyHashedPassword(null!, hashedPassword, password);

            return result == PasswordVerificationResult.Success;
        }
    }
}
