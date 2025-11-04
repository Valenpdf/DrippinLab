using Microsoft.CodeAnalysis.Scripting;
using BCrypt.Net;


namespace Drippin.Service
{
    // Encrypt.cs (dentro de la Carpeta Service)

    

    public static class Encrypt
    {
        // Genera un hash seguro de la contraseña usando un Salt aleatorio
        // y un factor de trabajo alto (costo) por defecto.
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contraseña no puede estar vacía.");

            // Usa BCrypt.HashPassword. El resultado incluye tanto el salt como el hash.
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Verifica la contraseña. BCrypt extrae el salt del hash almacenado 
        // y realiza la verificación lenta.
        public static bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
                return false;

            // BCrypt.Verify devuelve true si la contraseña de entrada coincide con el hash almacenado.
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
