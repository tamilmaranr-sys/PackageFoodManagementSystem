using System;
using System.Security.Cryptography;

namespace PackageFoodManagementSystem.Services.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string HashPassword(string password)
        {
            
            // Generate a random salt
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derive a 256-bit subkey (PBKDF2 with 100,000 iterations)
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                // Combine salt + hash
                byte[] hashBytes = new byte[48];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 32);

                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            // Extract salt
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Extract stored hash
            byte[] storedSubkey = new byte[32];
            Array.Copy(hashBytes, 16, storedSubkey, 0, 32);

            // Hash entered password with same salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 100000, HashAlgorithmName.SHA256))
            {
                byte[] enteredSubkey = pbkdf2.GetBytes(32);

                // Compare securely
                return CryptographicOperations.FixedTimeEquals(storedSubkey, enteredSubkey);
            }
        }
    }
}
