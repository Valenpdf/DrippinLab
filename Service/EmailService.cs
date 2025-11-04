using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Drippin.Models;
using System.Threading.Tasks;

namespace Drippin.Service
{
    // IEmailService (Interfaz que ya tienes)
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        // Inyección de la configuración de appsettings.json
        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_emailSettings.Username);
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.Username));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            // Crea la parte del cuerpo HTML (importante para enviar enlaces)
            var builder = new BodyBuilder
            {
                HtmlBody = message
            };

            email.Body = builder.ToMessageBody();

            using (var smtp = new SmtpClient())
            {
                try
                {
                    // Conexión y autenticación
                    await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);

                    // Envío
                    await smtp.SendAsync(email);
                }
                catch (Exception ex)
                {
                    // Manejo de errores (puedes loguear 'ex' o relanzar)
                    Console.WriteLine($"Error al enviar correo: {ex.Message}");
                    throw;
                }
                finally
                {
                    await smtp.DisconnectAsync(true);
                }
            }
        }
    }
}