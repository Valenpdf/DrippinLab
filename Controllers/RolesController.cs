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
    /// Gestiona las operaciones administrativas relacionadas con los roles de usuario en el sistema.
    /// Retorna vistas en: <see cref="Views.Roles"/>
    /// Utiliza: <see cref="Role"/>.
    /// </summary>
    public class RolesController : Controller
    {
        /// <summary>
        /// Contexto de acceso a datos para la gestión de roles.
        /// </summary>
        private readonly DrippinContext _context;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="RolesController"/>.
        /// </summary>
        /// <param name="context">Contexto de base de datos inyectado.</param>
        public RolesController(DrippinContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Presenta el listado de todos los roles registrados.
        /// Retorna: <see cref="Views.Roles.Index"/>
        /// </summary>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Role.ToListAsync());
        }

        /// <summary>
        /// Resuelve los detalles de un rol específico por su identificador.
        /// Retorna: <see cref="Views.Roles.Details"/>
        /// </summary>
        /// <param name="id">Identificador único del rol.</param>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Role
                .FirstOrDefaultAsync(m => m.IdRol == id);

            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        /// <summary>
        /// Presenta la vista para la creación de un nuevo rol.
        /// Retorna: <see cref="Views.Roles.Create"/>
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Procesa la inserción de un nuevo rol tras validar la integridad y unicidad del nombre provisto.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRol,NombreRol")] Role role)
        {
            if (ModelState.IsValid)
            {
                // Valida la unicidad del nombre del rol mediante una normalización de cadena.
                var nombreNormalizado = role.NombreRol.Trim().ToUpper();

                bool yaExiste = await _context.Role
                                    .AnyAsync(r => r.NombreRol.Trim().ToUpper() == nombreNormalizado);

                if (yaExiste)
                {
                    ModelState.AddModelError("NombreRol", "Ya existe un rol con este nombre.");
                    return View(role);
                }

                _context.Add(role);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        /// <summary>
        /// Presenta el formulario para la edición de un rol existente.
        /// Retorna: <see cref="Views.Roles.Edit"/>
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Role.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        /// <summary>
        /// Procesa la actualización de los datos de un rol existente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRol,NombreRol")] Role role)
        {
            if (id != role.IdRol)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Valida que el nombre actualizado no colisione con otros roles existentes.
                var nombreNormalizado = role.NombreRol.Trim().ToUpper();

                bool yaExiste = await _context.Role
                                    .AnyAsync(r => r.NombreRol.Trim().ToUpper() == nombreNormalizado && r.IdRol != role.IdRol);

                if (yaExiste)
                {
                    ModelState.AddModelError("NombreRol", "Ya existe otro rol con este nombre.");
                    return View(role);
                }

                try
                {
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoleExists(role.IdRol))
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
            return View(role);
        }

        /// <summary>
        /// Presenta la vista de confirmación para la eliminación de un rol.
        /// Retorna: <see cref="Views.Roles.Delete"/>
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Role
                .FirstOrDefaultAsync(m => m.IdRol == id);

            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        /// <summary>
        /// Procesa la eliminación definitiva de un rol de la base de datos.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.Role.FindAsync(id);
            if (role != null)
            {
                _context.Role.Remove(role);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Valida la existencia de un rol por su identificador.
        /// </summary>
        private bool RoleExists(int id)
        {
            return _context.Role.Any(e => e.IdRol == id);
        }
    }
}
