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
    public class CategoriasController : BaseController
    {
        public CategoriasController(DrippinContext context) : base(context)
        {
        }

        // GET: Categorias
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categoria.ToListAsync());
        }

        // GET: Categorias/Details/5
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

            // 2. APLICAR LÓGICA DE FILTRADO Y ORDENAMIENTO
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
                    // Ordenamiento por defecto si no hay filtro (ej. por nombre A-Z o fecha)
                    productosQuery = productosQuery.OrderBy(p => Guid.NewGuid());
                    break;
            }

            // 3. ASIGNAR LA LISTA ORDENADA DE VUELTA A LA PROPIEDAD DE NAVEGACIÓN
            // (Importante: Se necesita asignar la lista final ordenada)
            categoria.Productos = productosQuery.ToList();

            return View(categoria);
        }

        // GET: Categorias/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoriaId,CatNombre, CatICO")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categorias/Edit/5
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

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoriaId,CatNombre, CatICO")] Categoria categoria)
        {
            if (id != categoria.CategoriaId)
            {
                return NotFound();
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

        // GET: Categorias/Delete/5
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

        // POST: Categorias/Delete/5
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

        private bool CategoriaExists(int id)
        {
            return _context.Categoria.Any(e => e.CategoriaId == id);
        }
    }
}