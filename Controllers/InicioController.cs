using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Drippin.DTO; // Para UsuarioDTO
using System.Security.Claims;

namespace Drippin.Controllers
{
    [Authorize]
    public class InicioController : Controller
    {
        // ... Constructor e inyecciones ...

        [HttpGet]
        public IActionResult Index()
        {
            // 1. EXTRAER CLAIMS (Manera estándar y recomendada de obtener datos)

            // a. Datos estándar y extraídos directamente del ClaimTypes
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var nombreClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            var rolClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            // b. Datos específicos definidos en el Login
            var apellidoClaim = User.FindFirst("UsApellidoClaim")?.Value;
            var correoClaim = User.FindFirst("UsCorreoClaim")?.Value;

            // 2. USO DE VIEW BAG (Para pasar datos de forma simple a la vista)

            // Pasando las Claims directamente a ViewBag
            ViewBag.IdUsuario = idClaim;
            ViewBag.UsNombre = nombreClaim;
            ViewBag.UsCorreo = correoClaim;
            ViewBag.UsApellido = apellidoClaim;
            ViewBag.Role = rolClaim;

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult PanelAdmin()
        {
            ViewData["Message"] = "Bienvenido al Panel de Administrador.";
            return View();
        }

        // Solo accesible para usuarios cuya Claim de Role sea "Cliente"
        [Authorize(Roles = "Cliente")]
        [HttpGet]
        public IActionResult PerfilCliente()
        {
            ViewData["Message"] = "Bienvenido a tu Perfil de Cliente.";
            return View();
        }

        // Vista de Acceso Denegado (403 Forbidden)
        // El middleware de autenticación redirige aquí si un usuario logueado
        // intenta acceder a una acción con un rol que no posee (ej: un Cliente a PanelAdmin).
        [HttpGet]
        [AllowAnonymous] // Debe ser accesible por todos (incluso logueados)
        public IActionResult AccesoDenegado()
        {
            return View(); // Debes crear la vista Views/Inicio/AccesoDenegado.cshtml
        }
    }
}
