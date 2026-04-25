using Drippin.Data;
using Drippin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using System.Linq; 

namespace Drippin.Controllers
{
    /// <summary>
    /// Controlador principal encargado de gestionar la experiencia de usuario en la página de inicio,
    /// resolviendo el catálogo destacado, colecciones activas y estados globales del e-commerce.
    /// Retorna vistas en: <see cref="Views.Home"/>
    /// Utiliza: <see cref="Producto"/>, <see cref="Tanda"/> y <see cref="ErrorViewModel"/>.
    /// </summary>
    public class HomeController : BaseController
    {
        /// <summary>
        /// Servicio de registro de eventos para el seguimiento de la actividad del controlador.
        /// </summary>
        private readonly ILogger<HomeController> _logger;
        
        /// <summary>
        /// Inicializa una nueva instancia de <see cref="HomeController"/>.
        /// </summary>
        /// <param name="logger">Proveedor de logging inyectado.</param>
        /// <param name="context">Contexto de base de datos inyectado.</param>
        public HomeController(ILogger<HomeController> logger, DrippinContext context) : base(context)
        {
            _logger = logger;
        }

        /// <summary>
        /// Procesa la solicitud para la página principal, orquestando la visualización del producto destacado,
        /// el catálogo general con capacidades de ordenamiento y las tandas (colecciones) habilitadas.
        /// Retorna: <see cref="Views.Home.Index"/>
        /// </summary>
        /// <param name="sortBy">Criterio de ordenamiento para la grilla de productos.</param>
        public IActionResult Index(string sortBy)
        {
            // Resuelve el producto principal a destacar en la portada del sitio.
            var productoDestacado = _context.Producto
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.EsDestacado);

            // Prepara la consulta base filtrando productos no disponibles o asignados a colecciones activas.
            IQueryable<Producto> productosQuery = _context.Producto
                .Where(p => p.Disponible 
                       && (productoDestacado == null || p.proId != productoDestacado.proId)
                       && (p.Tanda == null || !p.Tanda.TanVisible));

            // Aplica cláusulas de ordenamiento dinámico según el parámetro recibido.
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
                    // Utiliza un ordenamiento pseudo-aleatorio ante la ausencia de criterios explícitos.
                    productosQuery = productosQuery.OrderBy(p => Guid.NewGuid());
                    break;
            }

            var productosLista = productosQuery.ToList();

            // Gestiona estados de vista para inventarios vacíos o catálogos sin productos.
            if (productoDestacado == null && productosLista.Count == 0)
            {
                ViewBag.MensajeSinProductos = "No hay productos para mostrar.";
                ViewBag.Productos = new List<Producto>();
            }
            else
            {
                ViewBag.Productos = productosLista;
            }

            // Resuelve las tandas (colecciones) visibles integrando sus productos asociados para mitigar latencia.
            ViewBag.Tandas = _context.Tanda
                .Where(t => t.TanVisible)
                .Include(t => t.Productos)
                .OrderBy(t => t.TandaId)
                .ToList();

            return View(productoDestacado);
        }

        /// <summary>
        /// Presenta la sección de políticas de privacidad del sitio.
        /// Retorna: <see cref="Views.Home.Privacy"/>
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Gestiona la presentación de errores globales del sistema, desactivando el almacenamiento en caché.
        /// Retorna: <see cref="Views.Shared.Error"/>
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}