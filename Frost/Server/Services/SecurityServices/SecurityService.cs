using Frost.Server.EntityModels;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Frost.Server.Services.AuthServices
{
    public class SecurityService
    {
        public static (string hashedPassword, string salt) GetHashedPasswordAndSalt(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            string saltBase64 = Convert.ToBase64String(salt);

            return new(hashed, saltBase64);
        }
        public static string GetHashedPasswordWithSalt(string password, string salt)
        {
            if (string.IsNullOrEmpty(password))
            {
                return "";
            }
            byte[] saltBytes = Convert.FromBase64String(salt);

            string currentHashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return currentHashedPassword;
        }
    }
}
