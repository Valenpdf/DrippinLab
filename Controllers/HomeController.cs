using Drippin.Data;
using Drippin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using System.Linq; // Asegúrate de tener este using para LINQ

namespace Drippin.Controllers
{
    // Heredamos de BaseController
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        
        // Recibe ILogger y DrippinContext.
        // Pasa el 'context' a la clase base (: base(context)).
        // Asigna el ILogger (ya que es específico de Home).
        public HomeController(ILogger<HomeController> logger, DrippinContext context) : base(context)
        {
            _logger = logger;
            // El _context es manejado por el constructor de BaseController.
        }

        public IActionResult Index(string sortBy)
        {
            
            // usamos _context. (Heredado de BaseController)

            // Obtiene un producto destacado
            var productoDestacado = _context.Producto
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.EsDestacado);

            // Consulta inicial para productos no destacados y disponibles
            IQueryable<Producto> productosQuery = _context.Producto
                .Where(p => p.Disponible && (productoDestacado == null || p.proId != productoDestacado.proId));

            // Aplicar ordenamiento según el parámetro 'sortBy'
            switch (sortBy)
            {
                case "precio_asc":
                    productosQuery = productosQuery.OrderBy(p => p.proPrecio);
                    break;
                case "precio_desc":
                    productosQuery = productosQuery.OrderByDescending(p => p.proPrecio);
                    break;
                case "nombre_asc":
                    productosQuery = productosQuery.OrderBy(p => p.proNombre);
                    break;
                case "nombre_desc":
                    productosQuery = productosQuery.OrderByDescending(p => p.proNombre);
                    break;
                default:
                    // Ordenamiento por defecto si no se especifica o el valor no es válido (ej. aleatorio o por fecha)
                    productosQuery = productosQuery.OrderBy(p => Guid.NewGuid());
                    break;
            }

            // También pasa la lista de productos normales si está en el ViewBag
            ViewBag.Productos = productosQuery.ToList();

            
                /* --- SI NO HAY PRODUCTOS EN LA BD --- */
            var productosLista = productosQuery.ToList();

            /* Verifica si no hay NINGÚN producto (ni el destacado ni productos normales) */
            if (productoDestacado == null && productosLista.Count == 0)
            {
                /* Muestra un mensaje en la vista */
                ViewBag.MensajeSinProductos = "No hay productos para mostrar.";
                /* Asegura que ViewBag.Productos sea una lista vacía */
                ViewBag.Productos = new List<Producto>();
            }
            else
            {
                /* Asignar la lista de productos (vacía o con contenido) */
                ViewBag.Productos = productosLista;
            }
                                            

            return View(productoDestacado); // Esto es lo que recibe la vista como "Model"
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}