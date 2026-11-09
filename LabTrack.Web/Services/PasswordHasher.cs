using System.Security.Cryptography;

namespace LabTrack.Web.Services
{
    /// <summary>
    /// Hash de contraseñas con PBKDF2-SHA256.
    /// Mantiene compatibilidad con contraseñas antiguas en texto plano y las
    /// actualiza automáticamente a hash en el siguiente inicio de sesión correcto.
    /// </summary>
    public static class PasswordHasher
    {
        private const string Prefix = "pbkdf2-sha256";
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

            return string.Join('$',
                Prefix,
                Iterations.ToString(),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(key));
        }

        public static bool IsHashed(string? stored)
            => !string.IsNullOrEmpty(stored) &&
               stored.StartsWith(Prefix + "$", StringComparison.Ordinal);

        public static bool Verify(string password, string? stored)
        {
            if (string.IsNullOrEmpty(stored))
            {
                return false;
            }

            // Compatibilidad con registros antiguos en texto plano.
            if (!IsHashed(stored))
            {
                return string.Equals(password, stored, StringComparison.Ordinal);
            }

            string[] parts = stored.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out int iterations))
            {
                return false;
            }

            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);
                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                    password, salt, iterations, Algorithm, expected.Length);

                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
