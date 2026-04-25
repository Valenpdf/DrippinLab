using Drippin.Data;
using Drippin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drippin.Controllers
{
    /// <summary>
    /// Gestiona las operaciones de administración de productos, incluyendo el filtrado,
    /// creación, edición y eliminación, así como el procesamiento de recursos multimedia asociados.
    /// Retorna vistas en: <see cref="Views.Productos"/>
    /// Utiliza: <see cref="Producto"/>, <see cref="Categoria"/> y <see cref="Tanda"/>.
    /// </summary>
    public class ProductosController : BaseController
    {
        /// <summary>
        /// Inicializa una nueva instancia de <see cref="ProductosController"/>.
        /// </summary>
        /// <param name="context">Contexto de base de datos inyectado.</param>
        public ProductosController(DrippinContext context) : base(context)
        {
        }

        /// <summary>
        /// Presenta la grilla de administración de productos con capacidades de filtrado por nombre e identificador de categoría.
        /// Retorna: <see cref="Views.Productos.Index"/>
        /// </summary>
        /// <param name="searchString">Término de búsqueda para el nombre del producto.</param>
        /// <param name="categoriaId">Identificador único de la categoría para el filtrado.</param>
        public async Task<IActionResult> Index(string searchString, int? categoriaId)
        {
            // Prepara la consulta base integrando la relación de categoría.
            IQueryable<Drippin.Models.Producto> productos = _context.Producto.Include(p => p.Categoria);

            // Aplica filtros dinámicos según los parámetros de búsqueda y categoría provistos.
            if (!String.IsNullOrEmpty(searchString))
            {
                ViewData["CurrentFilter"] = searchString;
                productos = productos.Where(p => p.proNombre.Contains(searchString));
            }

            if (categoriaId.HasValue)
            {
                ViewData["CurrentCategoriaId"] = categoriaId.Value;
                productos = productos.Where(p => p.CategoriaId == categoriaId.Value);
            }

            // Provee los metadatos necesarios para los controles de selección en la vista.
            ViewData["CategoriaId"] = new SelectList(_context.Categoria, "CategoriaId", "CatNombre", categoriaId);

            return View(await productos.ToListAsync());
        }

        /// <summary>
        /// Resuelve y presenta los detalles completos de un producto específico.
        /// Retorna: <see cref="Views.Productos.Details"/>
        /// </summary>
        /// <param name="id">Identificador único del producto.</param>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Producto
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.proId == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        /// <summary>
        /// Presenta la vista con el formulario para la creación de un nuevo producto.
        /// Retorna: <see cref="Views.Productos.Create"/>
        /// </summary>
        public IActionResult Create()
        {
            ViewBag.CategoriaId = new SelectList(_context.Categoria, "CategoriaId", "CatNombre");
            return View();
        }

        /// <summary>
        /// Procesa la creación de un nuevo producto, gestionando la validación de datos y el almacenamiento de recursos visuales.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("proId,proNombre,proPrecio,CategoriaId,proImagen,proImagen2,proImagen3,proImagen4,Disponible,EsDestacado")] Producto producto,
            IFormFile imagenSubida,
            IFormFile imagenSecundaria2,
            IFormFile imagenSecundaria3,
            IFormFile imagenSecundaria4)
        {
            // Remueve el estado de validación para las propiedades procesadas manualmente mediante flujo de archivos.
            if (ModelState.ContainsKey("proImagen")) { ModelState.Remove("proImagen"); }
            if (ModelState.ContainsKey("imagenSecundaria2")) { ModelState.Remove("imagenSecundaria2"); }
            if (ModelState.ContainsKey("imagenSecundaria3")) { ModelState.Remove("imagenSecundaria3"); }
            if (ModelState.ContainsKey("imagenSecundaria4")) { ModelState.Remove("imagenSecundaria4"); }

            // Valida la obligatoriedad del recurso visual principal.
            if (imagenSubida == null || imagenSubida.Length == 0)
            {
                ModelState.AddModelError("imagenSubida", "La imagen principal es obligatoria para la creación del producto.");
            }

            // Valida la unicidad del nombre del producto.
            if (producto.proNombre != null)
            {
                var nombreNormalizado = producto.proNombre.Trim().ToUpper();
                bool yaExiste = await _context.Producto
                                    .AnyAsync(p => p.proNombre.Trim().ToUpper() == nombreNormalizado);

                if (yaExiste)
                {
                    ModelState.AddModelError("proNombre", "Ya existe un producto con este nombre.");
                }
            }

            if (ModelState.IsValid)
            {
                // Almacena físicamente los archivos y persiste las rutas relativas en la base de datos.
                producto.proImagen = await GuardarImagen(imagenSubida);
                producto.proImagen2 = await GuardarImagen(imagenSecundaria2);
                producto.proImagen3 = await GuardarImagen(imagenSecundaria3);
                producto.proImagen4 = await GuardarImagen(imagenSecundaria4);

                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoriaId = new SelectList(_context.Categoria, "CategoriaId", "CatNombre", producto.CategoriaId);
            return View(producto);
        }

        /// <summary>
        /// Presenta el formulario para la edición de un producto existente, permitiendo la gestión de tandas y categorías.
        /// Retorna: <see cref="Views.Productos.Edit"/>
        /// </summary>
        /// <param name="id">Identificador único del producto.</param>
        public async Task<IActionResult> Edit(int? id)
        {
            var producto = await _context.Producto.FindAsync(id);
            ViewData["Action"] = "Edit";

            if (id == null || producto == null)
            {
                return NotFound();
            }

            ViewBag.CategoriaId = new SelectList(_context.Categoria, "CategoriaId", "CatNombre", producto.CategoriaId);
            ViewBag.TandaId = new SelectList(_context.Tanda, "TandaId", "TanNombre", producto.proTandaId);

            return View(producto);
        }

        /// <summary>
        /// Procesa la actualización de un producto existente, incluyendo la sustitución o eliminación selectiva de recursos visuales.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("EsDestacado, proId,proNombre,proPrecio,CategoriaId,proImagen,proImagen2,proImagen3,proImagen4,Disponible,proTandaId")] Producto producto,
            IFormFile imagenSubida,
            IFormFile imagenSecundaria2,
            IFormFile imagenSecundaria3,
            IFormFile imagenSecundaria4,
            bool eliminarImagenPrincipal = false,
            bool eliminarImagen2 = false,
            bool eliminarImagen3 = false,
            bool eliminarImagen4 = false)
        {
            if (id != producto.proId)
            {
                return NotFound();
            }

            // Recupera el estado original del registro para la gestión de recursos sin seguimiento de cambios.
            var productoActual = await _context.Producto.AsNoTracking().FirstOrDefaultAsync(p => p.proId == id);
            if (productoActual == null)
            {
                return NotFound();
            }

            // Limpia el estado de validación para las propiedades de multimedia.
            if (ModelState.ContainsKey("proImagen")) { ModelState.Remove("proImagen"); }
            if (ModelState.ContainsKey("imagenSubida")) { ModelState.Remove("imagenSubida"); }
            if (ModelState.ContainsKey("imagenSecundaria2")) { ModelState.Remove("imagenSecundaria2"); }
            if (ModelState.ContainsKey("imagenSecundaria3")) { ModelState.Remove("imagenSecundaria3"); }
            if (ModelState.ContainsKey("imagenSecundaria4")) { ModelState.Remove("imagenSecundaria4"); }

            // Gestiona la lógica de persistencia o reemplazo para el recurso visual principal.
            if (imagenSubida == null || imagenSubida.Length == 0)
            {
                if (eliminarImagenPrincipal)
                {
                    ModelState.AddModelError("imagenSubida", "No puede eliminar la imagen principal sin proveer una de reemplazo.");
                    producto.proImagen = productoActual.proImagen;
                }
                else if (string.IsNullOrEmpty(productoActual.proImagen))
                {
                    ModelState.AddModelError("imagenSubida", "La imagen principal es obligatoria para la edición del producto.");
                }
                else
                {
                    producto.proImagen = productoActual.proImagen;
                }
            }

            // Procesa selectivamente las imágenes secundarias basándose en los indicadores de eliminación o nuevos archivos.
            producto.proImagen2 = eliminarImagen2 ? null : (await GuardarImagen(imagenSecundaria2) ?? productoActual.proImagen2);
            producto.proImagen3 = eliminarImagen3 ? null : (await GuardarImagen(imagenSecundaria3) ?? productoActual.proImagen3);
            producto.proImagen4 = eliminarImagen4 ? null : (await GuardarImagen(imagenSecundaria4) ?? productoActual.proImagen4);

            // Valida la unicidad del nombre frente a otros registros existentes.
            if (producto.proNombre != null)
            {
                var nombreNormalizado = producto.proNombre.Trim().ToUpper();
                bool yaExisteEnOtro = await _context.Producto
                                    .AnyAsync(p => p.proNombre.Trim().ToUpper() == nombreNormalizado && p.proId != producto.proId);

                if (yaExisteEnOtro)
                {
                    ModelState.AddModelError("proNombre", "Ya existe otro producto con este nombre.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imagenSubida != null && imagenSubida.Length > 0)
                    {
                        producto.proImagen = await GuardarImagen(imagenSubida);
                    }

                    producto.prodCreacion = productoActual.prodCreacion;

                    // Implementa la política de exclusividad para el producto destacado.
                    if (producto.EsDestacado)
                    {
                        var destacadosAnteriores = await _context.Producto
                            .Where(p => p.EsDestacado && p.proId != producto.proId)
                            .ToListAsync();

                        foreach (var p in destacadosAnteriores)
                        {
                            p.EsDestacado = false;
                        }
                    }

                    _context.Update(producto);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.proId)) { return NotFound(); }
                    else { throw; }
                }
            }

            ViewData["Action"] = "Edit";
            ViewBag.CategoriaId = new SelectList(_context.Categoria, "CategoriaId", "CatNombre", producto.CategoriaId);
            ViewBag.TandaId = new SelectList(_context.Tanda, "TandaId", "TanNombre", producto.proTandaId);
            return View(producto);
        }

        /// <summary>
        /// Persiste un flujo de archivo en el sistema de almacenamiento y retorna la ruta relativa de acceso.
        /// </summary>
        /// <param name="imagenSubida">Archivo recibido desde la solicitud HTTP.</param>
        /// <returns>Ruta virtual del recurso almacenado o null si no se proveyó un archivo válido.</returns>
        private async Task<string> GuardarImagen(IFormFile imagenSubida)
        {
            if (imagenSubida != null && imagenSubida.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "drippin");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imagenSubida.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                Directory.CreateDirectory(uploadsFolder);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imagenSubida.CopyToAsync(fileStream);
                }
                return "~/images/drippin/" + uniqueFileName;
            }
            return null;
        }

        /// <summary>
        /// Presenta la vista de confirmación para la eliminación de un producto.
        /// Retorna: <see cref="Views.Productos.Delete"/>
        /// </summary>
        /// <param name="id">Identificador único del producto.</param>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Producto
                .FirstOrDefaultAsync(m => m.proId == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        /// <summary>
        /// Procesa la eliminación definitiva de un producto del catálogo.
        /// </summary>
        /// <param name="id">Identificador único del producto a eliminar.</param>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Producto.FindAsync(id);
            if (producto != null)
            {
                _context.Producto.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Valida la existencia concurrente de un producto por su identificador.
        /// </summary>
        private bool ProductoExists(int id)
        {
            return _context.Producto.Any(e => e.proId == id);
        }
    }
}