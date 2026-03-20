using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace VehicleRegistryAPI.Tools.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        // Genera un hash seguro para la contraseña
        public string HashPassword(string password)
        {
            // Generar un salt aleatorio de 128 bits (16 bytes)
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derivar el hash usando PBKDF2
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32 // 256 bits
            );

            // Guardar salt y hash juntos (Base64)
            string saltBase64 = Convert.ToBase64String(salt);
            string hashBase64 = Convert.ToBase64String(hash);

            return $"{saltBase64}.{hashBase64}";
        }

        // Verifica si la contraseña coincide con el hash almacenado
        public  bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] hashStored = Convert.FromBase64String(parts[1]);

            // Derivar el hash de la contraseña ingresada usando el mismo salt
            byte[] hashInput = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32
            );

            // Comparar byte a byte
            return CryptographicOperations.FixedTimeEquals(hashInput, hashStored);
        }
    }
}
