using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Drippin.Models;
using System.Threading.Tasks;

namespace Drippin.Service
{
    /// <summary>
    /// Define el contrato para el servicio de mensajería electrónica de la aplicación.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Realiza el envío asíncrono de un correo electrónico.
        /// </summary>
        /// <param name="toEmail">Dirección de destino.</param>
        /// <param name="subject">Asunto del mensaje.</param>
        /// <param name="message">Contenido del cuerpo del mensaje (soporta HTML).</param>
        Task SendEmailAsync(string toEmail, string subject, string message);
    }

    /// <summary>
    /// Implementación del servicio de mensajería utilizando el protocolo SMTP.
    /// Utilizado principalmente en: <see cref="Controllers.AccesosController"/>.
    /// </summary>
    public class EmailService : IEmailService
    {
        #region Atributos y Propiedades

        /// <summary>
        /// Configuración del servidor de correo obtenida desde la inyección de dependencias.
        /// </summary>
        private readonly EmailSettings _emailSettings;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="EmailService"/> con la configuración provista.
        /// </summary>
        /// <param name="emailSettings">Proveedor de opciones para la configuración de SMTP.</param>
        public EmailService(IOptions<EmailSettings> emailSettings)
        {   
            _emailSettings = emailSettings.Value;
        }

        #endregion

        #region Métodos de Envío

        /// <summary>
        /// Orquesta el proceso de conexión, autenticación y envío de un correo electrónico mediante un cliente SMTP.
        /// </summary>
        /// <param name="toEmail">Dirección de destino.</param>
        /// <param name="subject">Asunto del mensaje.</param>
        /// <param name="message">Contenido del cuerpo del mensaje en formato HTML.</param>
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_emailSettings.Username);
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.Username));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = message
            };

            email.Body = builder.ToMessageBody();

            using (var smtp = new SmtpClient())
            {
                try
                {
                    // Establecimiento de conexión con el servidor SMTP utilizando TLS.
                    await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
                    
                    // Autenticación de credenciales del remitente.
                    await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);

                    // Transferencia del mensaje al servidor para su distribución.
                    await smtp.SendAsync(email);
                }
                catch (Exception ex)
                {
                    // Registro técnico del fallo en el envío de la comunicación.
                    Console.WriteLine($"Error al enviar correo: {ex.Message}");
                    throw;
                }
                finally
                {
                    // Cierre de la conexión de red con el servidor de correo.
                    await smtp.DisconnectAsync(true);
                }
            }
        }

        #endregion
    }
}