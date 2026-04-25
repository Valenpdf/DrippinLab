using Drippin.Data;
using Drippin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Drippin.Controllers
{
    // Solo los Administradores pueden gestionar las Tandas.
    [Authorize(Roles = "Administrador")]
    /// <summary>
    /// Gestiona el ciclo de vida de las Tandas (colecciones de productos), permitiendo su creación,
    /// edición, eliminación y control de visibilidad en el catálogo. Restringido a administradores.
    /// Retorna vistas en: <see cref="Views.Tandas"/>
    /// Utiliza: <see cref="Tanda"/>.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class TandasController : BaseController
    {
        /// <summary>
        /// Inicializa una nueva instancia de <see cref="TandasController"/>.
        /// </summary>
        /// <param name="context">Contexto de base de datos inyectado.</param>
        public TandasController(DrippinContext context) : base(context)
        {
        }

        /// <summary>
        /// Presenta el listado completo de tandas registradas, incluyendo la carga de productos asociados.
        /// Retorna: <see cref="Views.Tandas.Index"/>
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Recupera las tandas ordenadas por identificador de forma descendente.
            var tandas = await _context.Tanda
                .Include(t => t.Productos)
                .OrderByDescending(t => t.TandaId)
                .ToListAsync();

            return View(tandas);
        }

        /// <summary>
        /// Presenta la vista con el formulario para la creación de una nueva tanda.
        /// Retorna: <see cref="Views.Tandas.Create"/>
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Procesa la creación de una nueva tanda tras validar la integridad de los datos.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TanNombre,TanVisible,TanFechaInicio,TanFechaFin")] Tanda tanda)
        {
            // Las fechas de vigencia son facultativas; se omiten errores de validación si no se proveen.
            if (ModelState.ContainsKey("TanFechaInicio")) { ModelState.Remove("TanFechaInicio"); }
            if (ModelState.ContainsKey("TanFechaFin")) { ModelState.Remove("TanFechaFin"); }

            if (ModelState.IsValid)
            {
                _context.Add(tanda);
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = $"La tanda \"{tanda.TanNombre}\" fue creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            return View(tanda);
        }

        /// <summary>
        /// Presenta el formulario para la edición de una tanda existente.
        /// Retorna: <see cref="Views.Tandas.Edit"/>
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tanda = await _context.Tanda.FindAsync(id);

            if (tanda == null)
            {
                return NotFound();
            }

            return View(tanda);
        }

        /// <summary>
        /// Procesa la actualización de los datos de una tanda existente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("TandaId,TanNombre,TanVisible,TanFechaInicio,TanFechaFin")] Tanda tanda)
        {
            if (id != tanda.TandaId)
            {
                return NotFound();
            }

            if (ModelState.ContainsKey("TanFechaInicio")) { ModelState.Remove("TanFechaInicio"); }
            if (ModelState.ContainsKey("TanFechaFin")) { ModelState.Remove("TanFechaFin"); }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tanda);
                    await _context.SaveChangesAsync();
                    TempData["MensajeExito"] = $"La tanda \"{tanda.TanNombre}\" fue actualizada exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TandaExists(tanda.TandaId)) { return NotFound(); }
                    else { throw; }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(tanda);
        }

        /// <summary>
        /// Alterna el estado de visibilidad de una tanda específica sin requerir la edición completa de la entidad.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibilidad(int id)
        {
            var tanda = await _context.Tanda.FindAsync(id);

            if (tanda == null)
            {
                return NotFound();
            }

            // Invierte el indicador de visibilidad pública de la tanda.
            tanda.TanVisible = !tanda.TanVisible;
            _context.Update(tanda);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Tanda \"{tanda.TanNombre}\" ahora está {(tanda.TanVisible ? "VISIBLE" : "OCULTA")} en el Index.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Presenta la vista de confirmación para la eliminación de una tanda, detallando el impacto en los productos asociados.
        /// Retorna: <see cref="Views.Tandas.Delete"/>
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tanda = await _context.Tanda
                .Include(t => t.Productos)
                .FirstOrDefaultAsync(m => m.TandaId == id);

            if (tanda == null)
            {
                return NotFound();
            }

            return View(tanda);
        }

        /// <summary>
        /// Procesa la eliminación de una tanda y gestiona la desvinculación de los productos relacionados.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tanda = await _context.Tanda
                .Include(t => t.Productos)
                .FirstOrDefaultAsync(t => t.TandaId == id);

            if (tanda != null)
            {
                // Disocia los productos de la tanda antes de proceder con la eliminación de la entidad principal.
                if (tanda.Productos != null)
                {
                    foreach (var producto in tanda.Productos)
                    {
                        producto.proTandaId = null;
                    }
                }

                _context.Tanda.Remove(tanda);
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = $"La tanda \"{tanda.TanNombre}\" fue eliminada correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Valida la existencia de una tanda en la base de datos por su identificador.
        /// </summary>
        private bool TandaExists(int id)
        {
            return _context.Tanda.Any(e => e.TandaId == id);
        }
    }
}
