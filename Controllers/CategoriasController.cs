using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Drippin.Data;
using Drippin.Models;

namespace Drippin.Controllers
{
    /// <summary>
    /// Gestiona las operaciones de administración y consulta de categorías de productos,
    /// incluyendo la visualización de productos asociados con capacidades de ordenamiento.
    /// Retorna vistas en: <see cref="Views.Categorias"/>
    /// Utiliza: <see cref="Categoria"/>.
    /// </summary>
    public class CategoriasController : BaseController
    {
        /// <summary>
        /// Inicializa una nueva instancia de <see cref="CategoriasController"/>.
        /// </summary>
        /// <param name="context">Contexto de base de datos inyectado.</param>
        public CategoriasController(DrippinContext context) : base(context)
        {
        }

        /// <summary>
        /// Presenta el listado completo de categorías registradas en el sistema.
        /// Retorna: <see cref="Views.Categorias.Index"/>
        /// </summary>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categoria.ToListAsync());
        }

        /// <summary>
        /// Resuelve los detalles de una categoría específica y su catálogo de productos asociado,
        /// aplicando criterios de ordenamiento según los parámetros provistos.
        /// Retorna: <see cref="Views.Categorias.Details"/>
        /// </summary>
        /// <param name="id">Identificador único de la categoría.</param>
        /// <param name="sortBy">Criterio de ordenamiento para los productos asociados.</param>
        public async Task<IActionResult> Details(int? id, string sortBy)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categoria
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(m => m.CategoriaId == id);

            if (categoria == null)
            {
                return NotFound();
            }

            // Implementa la lógica de filtrado y ordenamiento sobre el conjunto de productos asociados.
            IQueryable<Producto> productosQuery = categoria.Productos.AsQueryable();

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
                    // Aplica un ordenamiento aleatorio por defecto si no se especifica un criterio.
                    productosQuery = productosQuery.OrderBy(p => Guid.NewGuid());
                    break;
            }

            // Actualiza la propiedad de navegación con la colección de productos debidamente ordenada.
            categoria.Productos = productosQuery.ToList();

            return View(categoria);
        }

        /// <summary>
        /// Presenta la vista con el formulario para la creación de una nueva categoría.
        /// Retorna: <see cref="Views.Categorias.Create"/>
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Procesa la creación de una nueva entidad de categoría tras validar la integridad de los datos.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoriaId,CatNombre, CatICO")] Categoria categoria)
        {
            if (categoria.CatNombre != null)
            {
                var nombreNormalizado = categoria.CatNombre.Trim().ToUpper();
                bool yaExiste = await _context.Categoria
                                    .AnyAsync(c => c.CatNombre.Trim().ToUpper() == nombreNormalizado);

                if (yaExiste)
                {
                    ModelState.AddModelError("CatNombre", "Ya existe una categoría con este nombre.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        /// <summary>
        /// Presenta el formulario para la edición de una categoría existente.
        /// Retorna: <see cref="Views.Categorias.Edit"/>
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            return View(categoria);
        }

        /// <summary>
        /// Procesa la actualización de los datos de una categoría existente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoriaId,CatNombre, CatICO")] Categoria categoria)
        {
            if (id != categoria.CategoriaId)
            {
                return NotFound();
            }

            // Valida que el nombre actualizado no colisione con otras categorías existentes.
            if (categoria.CatNombre != null)
            {
                var nombreNormalizado = categoria.CatNombre.Trim().ToUpper();

                bool yaExisteEnOtro = await _context.Categoria
                                    .AnyAsync(c => c.CatNombre.Trim().ToUpper() == nombreNormalizado && c.CategoriaId != categoria.CategoriaId);

                if (yaExisteEnOtro)
                {
                    ModelState.AddModelError("CatNombre", "Ya existe otra categoría con este nombre.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.CategoriaId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        /// <summary>
        /// Presenta la vista de confirmación para la eliminación de una categoría.
        /// Retorna: <see cref="Views.Categorias.Delete"/>
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categoria
                .FirstOrDefaultAsync(m => m.CategoriaId == id);
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        /// <summary>
        /// Procesa la eliminación definitiva de una categoría de la base de datos.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria != null)
            {
                _context.Categoria.Remove(categoria);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Valida la existencia de una categoría por su identificador.
        /// </summary>
        private bool CategoriaExists(int id)
        {
            return _context.Categoria.Any(e => e.CategoriaId == id);
        }
    }
}