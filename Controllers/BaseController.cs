using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Drippin.Data; // DbContext
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;


/* Este controlador base actúa como una clase "padre" de la que heredan otros controladores para no duplicar codigo. */
public abstract class BaseController : Controller
{
    protected readonly DrippinContext _context;

    /* El constructor BaseController debe ser llamado por los Controllers hijos para tener acceso inmediato a la
     * base de datos mediante _context sin necesidad de volver a inyectarlo. */
    public BaseController(DrippinContext context)
    {
        _context = context;
    }

    /* Lógica que se ejecuta ANTES de cualquier acción en cualquier Controller que herede de esta clase 
     * accede a la base de datos para pedir la lista de todas las categorias ordenadas por nombre, */
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Carga la data para el Layout y lo guarda en una viewbag.
        ViewBag.Categorias = _context.Categoria.OrderBy(c => c.CatNombre).ToList();

        base.OnActionExecuting(context);
    }

    /* método de ayuda para obtener el ID del usuario que ha iniciado sesión.*/
    public string GetCurrentUserId()
    {
        /* "User.FindFirst" accede a los Claims del usuario autenticado -- 
         *  despues busca específicamente el ClaimTypes.NameIdentifier, que es donde ASP.NET Identity
         *  guarda el ID de usuario por defecto. */
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado");
        }
        
        /* Acá devuelve el ID del usuario de los Claims como string. 
         * Principalmente se usa en el controlador de carritos para saber a quién
         * pertenecen los productos. */
        return userId;
    }

}