using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using TaskTracker.Domain.Interfaces;

namespace TaskTracker.Infrastructure.Services
{
    // Using Argon2Id
    public class PasswordHasherService : IPasswordHasherService
    {
        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                MemorySize = 64 * 1024,
                Iterations = 4
            };
            var hash = argon2.GetBytes(32);
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string storedHash)
        {
            var parts = storedHash.Split('.', 2);
            if (parts.Length != 2) return false;
            var salt = Convert.FromBase64String(parts[0]);
            var expected = parts[1];
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                MemorySize = 64 * 1024,
                Iterations = 4
            };
            var computed = argon2.GetBytes(32);
            return Convert.ToBase64String(computed) == expected;
        }
    }
}
