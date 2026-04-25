using Drippin.Data;
using Drippin.Models;
using Drippin.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drippin.Controllers
{
    /// <summary>
    /// Gestiona las operaciones administrativas de usuarios, permitiendo la consulta,
    /// creación, edición y eliminación de perfiles dentro del sistema.
    /// Retorna vistas en: <see cref="Views.Usuarios"/>
    /// Utiliza: <see cref="Usuario"/> y <see cref="Role"/>.
    /// </summary>
    public class UsuariosController : Controller
    {
        /// <summary>
        /// Contexto de acceso a datos para la gestión de usuarios.
        /// </summary>
        private readonly DrippinContext _context;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="UsuariosController"/>.
        /// </summary>
        /// <param name="context">Contexto de base de datos inyectado.</param>
        public UsuariosController(DrippinContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Presenta el listado completo de usuarios registrados. Acceso restringido a administradores.
        /// Retorna: <see cref="Views.Usuarios.Index"/>
        /// </summary>
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuario.ToListAsync());
        }

        /// <summary>
        /// Resuelve los detalles de un usuario específico por su identificador. Acceso restringido a administradores.
        /// Retorna: <see cref="Views.Usuarios.Details"/>
        /// </summary>
        /// <param name="id">Identificador único del usuario.</param>
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        /// <summary>
        /// Presenta la vista para la creación de un nuevo usuario, cargando los roles disponibles.
        /// Retorna: <see cref="Views.Usuarios.Create"/>
        /// </summary>
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create()
        {
            // Provee el listado de roles para la selección en el formulario.
            ViewBag.IdRol = new SelectList(await _context.Role.ToListAsync(), "IdRol", "NombreRol");

            var nuevoUsuario = new Usuario
            {
                FechaRegistro = DateTime.Now.Date
            };

            return View();
        }

        /// <summary>
        /// Procesa la creación de un nuevo usuario tras validar los datos y realizar el hasheo de la contraseña.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UsNombre,UsApellido,UsCorreo,PasswordHash,IdRol")] Usuario usuario)
        {
            if (ModelState.IsValid) 
            {
                try
                {
                    // Aplica algoritmos de cifrado a la contraseña y establece los metadatos de registro.
                    usuario.PasswordHash = Encrypt.HashPassword(usuario.PasswordHash);
                    usuario.FechaRegistro = DateTime.Now;

                    _context.Add(usuario);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el usuario: " + ex.Message);
                }
            }

            ViewBag.IdRol = new SelectList(await _context.Role.ToListAsync(), "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }

        /// <summary>
        /// Presenta el formulario para la edición de un usuario existente.
        /// Retorna: <see cref="Views.Usuarios.Edit"/>
        /// </summary>
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        /// <summary>
        /// Procesa la actualización de los datos de un usuario existente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,UsNombre,UsApellido,UsCorreo,PasswordHash,IdRol,FechaRegistro,Token,FechaExpiracionToken")] Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario))
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

            ViewBag.IdRol = new SelectList(_context.Role, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }

        /// <summary>
        /// Presenta la vista de confirmación para la eliminación de un usuario.
        /// Retorna: <see cref="Views.Usuarios.Delete"/>
        /// </summary>
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        /// <summary>
        /// Procesa la eliminación definitiva de un usuario de la base de datos.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuario.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Valida la existencia de un usuario por su identificador.
        /// </summary>
        private bool UsuarioExists(int id)
        {
            return _context.Usuario.Any(e => e.IdUsuario == id);
        }
    }
}
