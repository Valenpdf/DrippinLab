using Microsoft.CodeAnalysis.Scripting;
using BCrypt.Net;


namespace Drippin.Service
{
    /// <summary>
    /// Provee utilitarios de seguridad para el tratamiento de credenciales,
    /// implementando algoritmos de hashing unidireccional y verificación.
    /// Utilizado en: <see cref="Controllers.AccesosController"/>.
    /// </summary>
    public static class Encrypt
    {
        #region Métodos de Seguridad

        /// <summary>
        /// Genera una representación segura (hash) de una cadena de texto plana utilizando el algoritmo BCrypt.
        /// El proceso incluye la generación automática de un "salt" aleatorio.
        /// </summary>
        /// <param name="password">Cadena de texto original (contraseña en claro).</param>
        /// <returns>Hash de seguridad resultante.</returns>
        /// <exception cref="ArgumentException">Se lanza si la contraseña provista es nula o vacía.</exception>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contraseña no puede estar vacía.");

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Valida la correspondencia entre una contraseña en texto plano y un hash de seguridad almacenado.
        /// </summary>
        /// <param name="password">Contraseña candidata ingresada por el usuario.</param>
        /// <param name="passwordHash">Hash de referencia almacenado en la base de datos.</param>
        /// <returns>Verdadero si la contraseña es válida; de lo contrario, falso.</returns>
        public static bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        #endregion
    }
}
