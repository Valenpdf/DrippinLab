using BCrypt.Net;
using Drippin.Data;
using Drippin.DTO;
using Drippin.Models;
using Drippin.Models.ViewModel;
using Drippin.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Drippin.Controllers
{
    /// <summary>
    /// Gestiona la seguridad e identidad de los usuarios, incluyendo procesos de autenticación,
    /// registro de nuevos clientes y flujos de recuperación de credenciales.
    /// Retorna vistas en: <see cref="Views.Accesos"/>
    /// Utiliza: <see cref="LoginViewModel"/>, <see cref="RecoveryViewModel"/>, <see cref="RecoveryPasswordViewModel"/>, <see cref="Usuario"/> y <see cref="DTO.UsuariosDTO"/>.
    /// </summary>
    public class AccesosController : Controller
    {
        /// <summary>
        /// Contexto de acceso a datos para la persistencia de usuarios.
        /// </summary>
        private readonly DrippinContext _context;

        /// <summary>
        /// Servicio para el envío de notificaciones por correo electrónico.
        /// </summary>
        private readonly IEmailService _emailService;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="AccesosController"/>.
        /// </summary>
        public AccesosController(DrippinContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        #region Login y Logout

        /// <summary>
        /// Presenta el formulario de inicio de sesión.
        /// Retorna: <see cref="Views.Accesos.Login"/>
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            // Redirige al inicio si el usuario ya cuenta con una sesión activa.
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Inicio");
            }
            return View();
        }

        /// <summary>
        /// Procesa las credenciales de acceso y establece la identidad del usuario mediante cookies.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Recupera la entidad de usuario incluyendo su rol asociado.
            var usuario = await _context.Usuario
                                         .Include(u => u.Role)
                                         .FirstOrDefaultAsync(u => u.UsCorreo == model.UsCorreo);

            // Valida la existencia del usuario y la integridad de la contraseña.
            if (usuario == null || !Encrypt.VerifyPassword(model.Password, usuario.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Credenciales incorrectas o usuario no registrado.");
                return View(model);
            }

            // Define las declaraciones (claims) de identidad para el principal de seguridad.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.UsNombre),
                new Claim("UsApellidoClaim", usuario.UsApellido),
                new Claim("UsCorreoClaim", usuario.UsCorreo),
                new Claim(ClaimTypes.Email, usuario.UsCorreo), 
                new Claim(ClaimTypes.Role, usuario.Role.NombreRol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Configura las propiedades de la sesión, considerando la persistencia solicitada.
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.Recordarme
            };

            if (model.Recordarme)
            {
                authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7);
            }

            // Realiza el proceso de SignIn en el contexto HTTP.
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Inicio");
        }

        /// <summary>
        /// Finaliza la sesión del usuario actual y elimina la cookie de autenticación.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); 
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Recuperación de Contraseña

        /// <summary>
        /// Presenta la vista para iniciar el proceso de recuperación de contraseña.
        /// Retorna: <see cref="Views.Accesos.StartRecovery"/>
        /// </summary>
        [HttpGet]
        public IActionResult StartRecovery()
        {
            return View();
        }

        /// <summary>
        /// Inicia el flujo de recuperación generando un token de seguridad y enviando un enlace por correo.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartRecovery(RecoveryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.UsCorreo == model.UsCorreo);

            // Se aplica una política de respuesta genérica para evitar la enumeración de usuarios.
            if (usuario != null)
            {
                // Generación de token único y establecimiento de ventana de expiración.
                string token = Guid.NewGuid().ToString("N");
                DateTime expiracion = DateTime.Now.AddHours(2);

                usuario.Token = token;
                usuario.FechaExpiracionToken = expiracion;
                await _context.SaveChangesAsync();

                // Construcción de la URL de recuperación y envío del correo electrónico.
                var recoveryUrl = Url.Action("Recovery", "Accesos", new { Token = token }, Request.Scheme);
                string subject = "Recuperación de Contraseña para Drippin";

                string mensajeCorreo = $@"
                    <html>
                        <body>
                            <p>Hola {usuario.UsNombre},</p>
                            <p>Recibimos una solicitud para reestablecer la contraseña de tu cuenta.</p>
                            <p>Para continuar, hacé clic en el siguiente enlace. Este enlace es válido por 2 horas:</p>
                            <p><a href='{recoveryUrl}'><strong>Reestablecer mi Contraseña</strong></a></p>
                            <p>Si no solicitaste este cambio, por favor ignora este correo.</p>
                        </body>
                    </html>";

                await _emailService.SendEmailAsync(usuario.UsCorreo, subject, mensajeCorreo);
            }

            TempData["InfoMessage"] = "Si su correo está registrado, recibirá un enlace para reestablecer su contraseña.";
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Valida el token de recuperación y presenta el formulario para establecer una nueva contraseña.
        /// Retorna: <see cref="Views.Accesos.Recovery"/>
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Recovery(string Token)
        {
            if (string.IsNullOrEmpty(Token))
            {
                TempData["Error"] = "Token no proporcionado.";
                return RedirectToAction("Login");
            }

            // Verifica la validez del token y su vigencia temporal.
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Token == Token && u.FechaExpiracionToken > DateTime.Now);

            if (usuario == null)
            {
                TempData["Error"] = "El enlace no es válido o ha expirado. Solicite uno nuevo.";
                return RedirectToAction("Login");
            }

            return View(new RecoveryPasswordViewModel { Token = Token });
        }

        /// <summary>
        /// Procesa la actualización de la contraseña del usuario tras la validación del token.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recovery(RecoveryPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Token == model.Token && u.FechaExpiracionToken > DateTime.Now);

            if (usuario == null)
            {
                TempData["Error"] = "Error de validación. Intente solicitar un nuevo enlace.";
                return RedirectToAction("Login");
            }

            // Actualiza el hash de la contraseña e invalida el token utilizado.
            usuario.PasswordHash = Drippin.Service.Encrypt.HashPassword(model.NewPassword);
            usuario.Token = "tokenbloqueado";
            usuario.FechaExpiracionToken = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Contraseña reestablecida con éxito. Inicie sesión.";
            return RedirectToAction("Login");
        }

        #endregion

        #region Registro

        /// <summary>
        /// Presenta la vista de registro para nuevos clientes.
        /// Retorna: <see cref="Views.Accesos.Registrarse"/>
        /// </summary>
        [HttpGet]
        public IActionResult Registrarse()
        {
            return View();
        }

        /// <summary>
        /// Procesa el registro de un nuevo usuario con el rol predeterminado de cliente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrarse(UsuariosDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Valida la unicidad del correo electrónico en el sistema.
            var usuarioExistente = await _context.Usuario.FirstOrDefaultAsync(u => u.UsCorreo == model.UsCorreo);
            if (usuarioExistente != null)
            {
                ModelState.AddModelError("UsCorreo", "Este correo ya está registrado en el sistema.");
                return View(model);
            }

            // Recupera la entidad de rol para 'Cliente'.
            var rolEntity = await _context.Role.FirstOrDefaultAsync(r => r.IdRol == 2); 

            if (rolEntity == null)
            {
                ModelState.AddModelError("", "El rol por defecto ('Cliente') no se encuentra en el sistema. Contacte al administrador.");
                return View(model);
            }

            // Crea la nueva entidad de usuario con los datos provistos y la contraseña hasheada.
            var nuevoUsuario = new Usuario
            {
                UsNombre = model.UsNombre,
                UsCorreo = model.UsCorreo,
                UsApellido = model.UsApellido,
                PasswordHash = Encrypt.HashPassword(model.Password),
                IdRol = rolEntity.IdRol,
                FechaRegistro = DateTime.Now
            };

            _context.Usuario.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registro exitoso. ¡Inicia sesión ahora!";
            return RedirectToAction("Login");
        }

        #endregion
        
    }
}
