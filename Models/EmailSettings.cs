namespace Drippin.Models
{
    // Datos sacados de appsettings para que el servicio de email funcione
    /// <summary>
    /// Define los parámetros de configuración necesarios para el servicio de mensajería electrónica,
    /// extrayendo los valores desde el archivo de configuración del sistema.
    /// </summary>
    public class EmailSettings
    {
        #region Configuración del Servidor SMTP

        /// <summary>
        /// Dirección del host del servidor SMTP.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Puerto de comunicación del servidor SMTP.
        /// </summary>
        public int Port { get; set; }

        #endregion

        #region Credenciales y Remitente

        /// <summary>
        /// Identificador de usuario para la autenticación en el servidor SMTP.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Contraseña asociada a la cuenta de autenticación SMTP.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Nombre descriptivo que aparecerá como remitente de los correos electrónicos.
        /// </summary>
        public string SenderName { get; set; }

        #endregion
    }
}
