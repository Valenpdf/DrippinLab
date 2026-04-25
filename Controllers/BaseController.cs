using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Drippin.Data; // DbContext
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;


/// <summary>
/// Provee una base funcional compartida para los controladores de la aplicación,
/// gestionando el contexto de datos y la preparación de metadatos globales (ej. categorías para el layout).
/// Heredado por: <see cref="Controllers.HomeController"/>, <see cref="Controllers.ProductosController"/>, etc.
/// </summary>
public abstract class BaseController : Controller
{
    /// <summary>
    /// Contexto de acceso a la base de datos compartido.
    /// </summary>
    protected readonly DrippinContext _context;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="BaseController"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos inyectado.</param>
    public BaseController(DrippinContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Ejecuta lógica transversal antes de la invocación de acciones, como la carga de categorías.
    /// </summary>
    /// <param name="context">Contexto de ejecución de la acción.</param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Recupera y ordena el catálogo de categorías para su visualización global en el layout.
        ViewBag.Categorias = _context.Categoria.OrderBy(c => c.CatNombre).ToList();

        base.OnActionExecuting(context);
    }

    /// <summary>
    /// Recupera el identificador único del usuario actualmente autenticado desde sus declaraciones (claims).
    /// </summary>
    /// <returns>El identificador del usuario como cadena de texto.</returns>
    /// <exception cref="UnauthorizedAccessException">Se lanza si el usuario no cuenta con una sesión activa.</exception>
    public string GetCurrentUserId()
    {
        // Resuelve el identificador a partir del claim NameIdentifier establecido durante el login.
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado");
        }
        
        return userId;
    }
}