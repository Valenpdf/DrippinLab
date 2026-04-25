using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Drippin.DTO; // Para UsuarioDTO
using System.Security.Claims;

namespace Drippin.Controllers
{
    /// <summary>
    /// Gestiona el redireccionamiento inicial tras la autenticación y provee acceso a paneles
    /// específicos según el rol del usuario, así como la gestión de accesos denegados.
    /// Retorna vistas en: <see cref="Views.Inicio"/>
    /// </summary>
    [Authorize]
    public class InicioController : Controller
    {
        

        /// <summary>
        /// Extrae las declaraciones (claims) del usuario autenticado y redirige a la página principal.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            // Recupera los identificadores y atributos de identidad del usuario actual a partir de sus declaraciones.
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var nombreClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            var rolClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            var apellidoClaim = User.FindFirst("UsApellidoClaim")?.Value;
            var correoClaim = User.FindFirst("UsCorreoClaim")?.Value;

            // Almacena los atributos de identidad en el contenedor ViewBag para su disponibilidad global en la vista.
            ViewBag.IdUsuario = idClaim;
            ViewBag.UsNombre = nombreClaim;
            ViewBag.UsCorreo = correoClaim;
            ViewBag.UsApellido = apellidoClaim;
            ViewBag.Role = rolClaim;

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Provee acceso al panel administrativo, restringido exclusivamente a usuarios con rol de Administrador.
        /// Retorna: <see cref="Views.Inicio.PanelAdmin"/>
        /// </summary>
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult PanelAdmin()
        {
            ViewData["Message"] = "Bienvenido al Panel de Administrador.";
            return View();
        }

        /// <summary>
        /// Presenta la vista de perfil para usuarios con rol de Cliente.
        /// Retorna: <see cref="Views.Inicio.PerfilCliente"/>
        /// </summary>
        [Authorize(Roles = "Cliente")]
        [HttpGet]
        public IActionResult PerfilCliente()
        {
            ViewData["Message"] = "Bienvenido a tu Perfil de Cliente.";
            return View();
        }

        /// <summary>
        /// Gestiona la presentación de la vista de acceso denegado cuando un usuario carece de los permisos necesarios.
        /// Retorna: <see cref="Views.Inicio.AccesoDenegado"/>
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}
