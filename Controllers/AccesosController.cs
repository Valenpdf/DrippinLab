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
    /* Este controlador maneja la seguridad e identidad de los usuarios, gestionando el login, registro,
     * logout y el flujo de recuperación de contraseña. */
    public class AccesosController : Controller
    {
        private readonly DrippinContext _context; /* Para consultar y guardar usuarios en la BD */
        private readonly IEmailService _emailService; /* Para enviar los correos de recuperación de contraseña. */

        public AccesosController(DrippinContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ------------------------- LOGIN ------------------------- //
        [HttpGet]
        public IActionResult Login() /* Muestra el formulario de Logueo. */
        {
            /* Si el usuario ya está autenticado, lo redirije al Inicio 
            *  para evitar que un usuario logueado vea el login. */
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Inicio");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) /* Si el formulario no se valida, vuelve al login. */
            {
                return View(model);
            }

            // Busca el usuario por correo, incluyendo su Rol para las Claims
            var usuario = await _context.Usuario
                                         .Include(u => u.Role)
                                         .FirstOrDefaultAsync(u => u.UsCorreo == model.UsCorreo);

            /* Verifica si el usuario existe y si la contraseña coincide, usando el método VerifyPassword del servicio Encrypt
             * de encriptación */
            if (usuario == null || !Encrypt.VerifyPassword(model.Password, usuario.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Credenciales incorrectas o usuario no registrado.");
                return View(model);
            }

            // Si el usuario existe y la contraseña coincide, crea las Claims (Identidad)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()), // ID único (ASP.Net Identity)
                new Claim(ClaimTypes.Name, usuario.UsNombre), // Nombre para mostrar
                new Claim("UsApellidoClaim", usuario.UsApellido), // Apellido del usuario
                new Claim("UsCorreoClaim", usuario.UsCorreo), // Correo
                new Claim(ClaimTypes.Email, usuario.UsCorreo), 
                new Claim(ClaimTypes.Role, usuario.Role.NombreRol) // CRUCIAL para [Authorize(Roles = "")]
            };

            /* Se crea la variable que contiene las Claims del usuario y las cookies */
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Propiedades de Autenticación (manejo de 'Recordarme')
            var authProperties = new AuthenticationProperties
            {
                /* Acá indica que las cookies de sesion se manejan desde "Recordarme" */
                IsPersistent = model.Recordarme, 

                /* Si Recordarme es true, la cookie de sesion dura 7 dias, sino, dura 30 minutos */
                ExpiresUtc = model.Recordarme ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddMinutes(30)
            };

            // Inicia la sesión la sesión del usuario, crea la Cookie y redirigiendo al inicio)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Inicio");
        }

        // 6. Cerrar Sesión del usuario (Logout)
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            /* Destruye la cookie de autenticacion */
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); 
            
            /* Redirige al Home publico */
            return RedirectToAction("Index", "Home");
        }


        // ------------------------- RECOVERY ------------------------- //

        // GET: Muestra el formulario para ingresar el correo
        [HttpGet]
        public IActionResult StartRecovery()
        {
            return View();
        }

        // Recibe el correo del usuario que olvidó su contraseña mediante el ViewModel de Recovery
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartRecovery(RecoveryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            /* Busca al usuario por su correo */
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.UsCorreo == model.UsCorreo);

            /* Da un mensaje genérico para no revelar si el correo existe (Asi se evita que un atacante use el formulario
             * para adivinar qué correos están registrados en el sistema) */
            if (usuario != null)
            {
                // Generar Token Único
                string token = Guid.NewGuid().ToString("N");

                // Establece una expiración de 2 horas
                DateTime expiracion = DateTime.Now.AddHours(2);

                // Guarda el token en la BD, en el registro del usuario.
                usuario.Token = token;
                usuario.FechaExpiracionToken = expiracion;
                await _context.SaveChangesAsync();

                // Generar un enlace de recuperación completo 
                var recoveryUrl = Url.Action("Recovery", "Accesos", new { Token = token }, Request.Scheme);

                string subject = "Recuperación de Contraseña para Drippin";

                // Crear un mensaje con formato HTML que incluye el enlace
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

                // Llama al método SendEmailAsync para enviar el correo con el enlace HTML.
                await _emailService.SendEmailAsync(usuario.UsCorreo, subject, mensajeCorreo);
            

            }

            TempData["InfoMessage"] = "Si su correo está registrado, recibirá un enlace para reestablecer su contraseña.";
            return RedirectToAction("Login");
        }

        /* Se activa cuando el usuario cliquea en el enlace del correo. */
        [HttpGet]
        public async Task<IActionResult> Recovery(string Token)
        {
            /* Si no existe un token, tira error. */
            if (string.IsNullOrEmpty(Token))
            {
                TempData["Error"] = "Token no proporcionado.";
                return RedirectToAction("Login");
            }

            /* Verifica que el token exista y que no haya expirado. */
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Token == Token && u.FechaExpiracionToken > DateTime.Now);

            if (usuario == null)
            {
                TempData["Error"] = "El enlace no es válido o ha expirado. Solicite uno nuevo.";
                return RedirectToAction("Login");
            }

            /* Si el token es válido, muestra el formulario de nueva contraseña */
            return View(new RecoveryPasswordViewModel { Token = Token });
        }

        /* Cambiar la contraseña */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recovery(RecoveryPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            /* Re-validar el token usando la misma lógica de existencia + expiración. */
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Token == model.Token && u.FechaExpiracionToken > DateTime.Now);

            if (usuario == null)
            {
                TempData["Error"] = "Error de validación. Intente solicitar un nuevo enlace.";
                return RedirectToAction("Login");
            }

            /* Si la re-validación falla, tira error, y sino:
             * hashea y actualiza la nueva contraseña */
            usuario.PasswordHash = Drippin.Service.Encrypt.HashPassword(model.NewPassword);

            /* E invalida el token para que no se pueda volver a usar. */
            usuario.Token = null;
            usuario.FechaExpiracionToken = null;

            /* Guarda los cambios en la base de datos con la nueva contraseña y token vacío. */
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Contraseña reestablecida con éxito. Inicie sesión.";

            /* Y redirige al Login. */
            return RedirectToAction("Login");
        }



        // -------------------- REGISTRO --------------------- //

        [HttpGet]
        public IActionResult Registrarse() /* Muestra el formulario de registro */
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrarse(UsuariosDTO model) /* Procesa el formulario de registro de un nuevo cliente. */
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Verifica si el correo ya existe, y si existe, muestra el mensaje de error
            var usuarioExistente = await _context.Usuario.FirstOrDefaultAsync(u => u.UsCorreo == model.UsCorreo);
            if (usuarioExistente != null)
            {
                ModelState.AddModelError("UsCorreo", "Este correo ya está registrado en el sistema.");
                return View(model);
            }

            /* Verifica si existe el rol con Id = 2 ('Cliente').  */
            var rolEntity = await _context.Role.FirstOrDefaultAsync(r => r.IdRol == 2); 

            if (rolEntity == null)
            {
                /* Manejo de error si el rol por defecto no existe en la base de datos
                 * (Aunque el rol ya se crea por si solo al crear la BD en el contexto.) */
                ModelState.AddModelError("", "El rol por defecto ('Cliente') no se encuentra en el sistema. Contacte al administrador.");
                return View(model);
                
            }

            // Hashea la contraseña insertada por el usuario en UsuariosDTO (usando BCrypt)
            string passwordHash = Encrypt.HashPassword(model.Password);

            // 3. Crear una instancia nueva del modelo Usuario y la llena con los datos del UsuariosDTO
            var nuevoUsuario = new Usuario
            {
                UsNombre = model.UsNombre,
                UsCorreo = model.UsCorreo,
                UsApellido = model.UsApellido,
                PasswordHash = passwordHash,
                IdRol = rolEntity.IdRol, // rol de 'Cliente'
                FechaRegistro = DateTime.Now
            };

            // Lo guarda en la base de datos
            _context.Usuario.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            // Redirige al Login
            TempData["SuccessMessage"] = "Registro exitoso. ¡Inicia sesión ahora!";
            return RedirectToAction("Login");

        }
    }
}
