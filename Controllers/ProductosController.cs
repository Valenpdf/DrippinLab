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
    public class ProductosController : BaseController
    {
        // Constructor: Recibe el DbContext y lo pasa a la clase base (BaseController).
        public ProductosController(DrippinContext context) : base(context)
        {
        }

        // GET: Productos - Muestra la lista de productos (Panel de Admin).
        public async Task<IActionResult> Index(string searchString, int? categoriaId)
        {
            // Obtiene la consulta inicial, incluyendo la navegación a Categoría
            // Inicia la consulta (IQueryable) incluyendo la Categoría. Aún no se ejecuta.
            IQueryable<Drippin.Models.Producto> productos = _context.Producto.Include(p => p.Categoria);

            // Aplicar filtro de búsqueda por nombre (si se proveyó).
            if (!String.IsNullOrEmpty(searchString))
            {
                // Guarda el filtro actual para rellenar la caja de búsqueda en la vista.
                ViewData["CurrentFilter"] = searchString;

                // Añade la condición 'Where' a la consulta IQueryable.
                productos = productos.Where(p => p.proNombre.Contains(searchString));
            }

            // FILTRO: Aplicar filtro por categoría (si se seleccionó una).
            if (categoriaId.HasValue)
            {
                // Guarda el ID de categoría para mantener la selección en el dropdown de la vista.
                ViewData["CurrentCategoriaId"] = categoriaId.Value;

                // Añade la condición 'Where' por CategoriaId a la consulta IQueryable.
                productos = productos.Where(p => p.CategoriaId == categoriaId.Value);
            }

            // Prepara el SelectList(dropdown) de categorías para el filtro de la vista.
            ViewData["CategoriaId"] = new SelectList(_context.Categoria, "CategoriaId", "CatNombre", categoriaId);

            // EJECUTA la consulta contra la BD (con todos los filtros) y pasa la lista a la vista.
               return View(await productos.ToListAsync());
        }

        // GET: Productos/Details/5 - Muestra los detalles de un producto.
        public async Task<IActionResult> Details(int? id)
        {
            // Si no se provee un ID, retorna 'No Encontrado'.
            if (id == null)
            {
                return NotFound();
            }

            // Busca el producto por ID en la BD, incluyendo su Categoría.
            var producto = await _context.Producto
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.proId == id);

            // Si el producto no existe en la BD, retorna 'No Encontrado'.
            if (producto == null)
            {
                return NotFound();
            }

            // Muestra la vista de Detalles con el producto encontrado.
            return View(producto);
        }

        // GET: Productos/Create - Muestra el formulario para crear un producto.
        public IActionResult Create()
        {
            // Prepara el dropdown de categorías para el formulario.
            ViewBag.CategoriaId = new SelectList(_context.Categoria, "CategoriaId", "CatNombre");
            return View();
        }

        // POST: Productos/Create - Procesa el envío del formulario de creación.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            // Recibe el modelo 'Producto' (con los campos del form) y los archivos de imagen (IFormFile).
            [Bind("proId,proNombre,proPrecio,CategoriaId,proImagen,proImagen2,proImagen3,proImagen4,Disponible,EsDestacado")] Producto producto,
            IFormFile imagenSubida, // Imagen principal (obligatoria)
            IFormFile imagenSecundaria2, // <--- Nueva imagen 2
            IFormFile imagenSecundaria3, // <--- Nueva imagen 3
            IFormFile imagenSecundaria4) // <--- Nueva imagen 4
        {

            // ---  Limpieza Completa de ModelState ---
            // Limpia los errores de todas las propiedades que se manejan manualmente
            // o que son propiedades de navegación que el formulario no envía.

            // Quita los errores de las propiedades 'proImagenX' (string)...
            if (ModelState.ContainsKey("proImagen")) { ModelState.Remove("proImagen"); }

            // ...y de los parámetros IFormFile opcionales, ya que se manejan manualmente.
            if (ModelState.ContainsKey("imagenSecundaria2")) { ModelState.Remove("imagenSecundaria2"); }
            if (ModelState.ContainsKey("imagenSecundaria3")) { ModelState.Remove("imagenSecundaria3"); }
            if (ModelState.ContainsKey("imagenSecundaria4")) { ModelState.Remove("imagenSecundaria4"); }

            // Validación Manual: Verifica si se subió la imagen principal.
            if (imagenSubida == null || imagenSubida.Length == 0)
            {
                ModelState.AddModelError("imagenSubida", "La imagen principal es obligatoria para la creación del producto.");
            }

            // Si el modelo (y la validación manual) es válido...
            if (ModelState.IsValid)
            {
                // Llama al método 'GuardarImagen' para procesar y guardar los archivos.
                producto.proImagen = await GuardarImagen(imagenSubida);

                // 'GuardarImagen' devuelve 'null' si el archivo es nulo (opcionales).
                producto.proImagen2 = await GuardarImagen(imagenSecundaria2);
                producto.proImagen3 = await GuardarImagen(imagenSecundaria3);
                producto.proImagen4 = await GuardarImagen(imagenSecundaria4);
                // -----------------------------------------------------

                // Agrega el nuevo producto al DbContext.
                _context.Add(producto);
                // Guarda los cambios en la BD.
                await _context.SaveChangesAsync();
                // Redirige a la lista de productos (Index).
                return RedirectToAction(nameof(Index));
            }

            // Si la validación falla, recarga el dropdown de categorías y muestra el formulario de nuevo.
            ViewBag.CategoriaId = new SelectList(_context.Categoria, "CategoriaId", "CatNombre", producto.CategoriaId);
            return View(producto);
        }

        // GET: Productos/Edit/5 - Muestra el formulario para editar un producto.
        public async Task<IActionResult> Edit(int? id)
        {
            // Busca el producto por ID (FindAsync es óptimo para buscar por Key).
            var producto = await _context.Producto.FindAsync(id);

            // (Opcional) Pasa un 'Action' a la vista, útil si se reutiliza el formulario de Create.
            ViewData["Action"] = "Edit";

            // Valida si se pasó un ID.
            if (id == null)
            {
                return NotFound();
            }

            // Valida si el producto con ese ID existe.
            if (producto == null)
            {
                return NotFound();
            }

            // Carga el dropdown de categorías, pre-seleccionando la categoría actual del producto.
            ViewBag.CategoriaId = new SelectList(_context.Categoria, "CategoriaId", "CatNombre", producto.CategoriaId);
            return View(producto);
        }

        // POST: Productos/Edit/5 - Procesa el envío del formulario de edición.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            // Recibe el modelo 'Producto', los *nuevos* archivos IFormFile y los *checkboxes* 'eliminarImagen'.
            [Bind("EsDestacado, proId,proNombre,proPrecio,CategoriaId,proImagen,proImagen2,proImagen3,proImagen4,Disponible")] Producto producto,
            IFormFile imagenSubida, // Principal
            IFormFile imagenSecundaria2,
            IFormFile imagenSecundaria3,
            IFormFile imagenSecundaria4,
            bool eliminarImagenPrincipal = false,
            bool eliminarImagen2 = false,
            bool eliminarImagen3 = false,
            bool eliminarImagen4 = false)
        {
            // Validación de seguridad: el ID de la ruta debe coincidir con el ID del modelo.
            if (id != producto.proId)
            {
                return NotFound();
            }

            // Obtiene los valores *originales* del producto desde la BD (sin rastreo).
            var productoActual = await _context.Producto.AsNoTracking().FirstOrDefaultAsync(p => p.proId == id);
            if (productoActual == null)
            {
                return NotFound();
            }

            // ---  Limpieza Completa de ModelState ---
            // Quita errores de propiedades de imagen (string y IFormFile) para manejo manual.
            
            if (ModelState.ContainsKey("proImagen")) { ModelState.Remove("proImagen"); }
            if (ModelState.ContainsKey("imagenSubida")) { ModelState.Remove("imagenSubida"); }
            if (ModelState.ContainsKey("imagenSecundaria2")) { ModelState.Remove("imagenSecundaria2"); }
            if (ModelState.ContainsKey("imagenSecundaria3")) { ModelState.Remove("imagenSecundaria3"); }
            if (ModelState.ContainsKey("imagenSecundaria4")) { ModelState.Remove("imagenSecundaria4"); }


            // --------------------------------------------------------


            // --- Lógica de Imagen Principal: Verifica si *no* se subió un archivo nuevo. ---
            if (imagenSubida == null || imagenSubida.Length == 0)
            {
                // No se subió archivo nuevo
                if (eliminarImagenPrincipal)
                {
                    // Marcó eliminar Y no subió uno nuevo. Error.
                    ModelState.AddModelError("imagenSubida", "No puede eliminar la imagen principal sin subir una de reemplazo.");
                    producto.proImagen = productoActual.proImagen; // Mantiene la actual para la vista
                }
                else if (string.IsNullOrEmpty(productoActual.proImagen))
                {
                    // No subió archivo nuevo Y no había uno antes. Error.
                    ModelState.AddModelError("imagenSubida", "La imagen principal es obligatoria para la edición del producto.");
                }
                else
                {
                    // No subió archivo nuevo, no marcó eliminar, y sí había uno. OK.
                    producto.proImagen = productoActual.proImagen;
                }
            }
            // Si se subió un archivo nuevo, la lógica en if(ModelState.IsValid) se encargará.


            // --- Lógica de Imágenes Secundarias (opcionales) ---

            // Imagen 2
            if (eliminarImagen2)
            {
                producto.proImagen2 = null; // Si el checkbox 'eliminar' está marcado, la pone en null.
            }
            else
            {
                // Si no, intenta guardar la nueva imagen. Si no se subió (null), mantiene la actual.
                producto.proImagen2 = await GuardarImagen(imagenSecundaria2) ?? productoActual.proImagen2;
            }

            // Imagen 3
            if (eliminarImagen3)
            {
                producto.proImagen3 = null; // Borrar
            }
            else
            {
                producto.proImagen3 = await GuardarImagen(imagenSecundaria3) ?? productoActual.proImagen3;
            }

            // Imagen 4
            if (eliminarImagen4)
            {
                producto.proImagen4 = null; // Borrar
            }
            else
            {
                producto.proImagen4 = await GuardarImagen(imagenSecundaria4) ?? productoActual.proImagen4;
            }

            // --------------------------------------------------------

            // 5. Si el modelo (y la validación manual) es válido...
            if (ModelState.IsValid)
            {
                try
                {
                    // --- Guardar la NUEVA imagen Principal (si se subió) ---
                    if (imagenSubida != null && imagenSubida.Length > 0)
                    {
                        producto.proImagen = await GuardarImagen(imagenSubida);
                    }

                    // Preserva la fecha de creación original.
                    producto.prodCreacion = productoActual.prodCreacion;


                    // Lógica de "Producto Destacado Único".
                    if (producto.EsDestacado)
                    {
                        // Busca todos los *otros* productos que estén destacados...
                        var destacadosAnteriores = await _context.Producto
                            .Where(p => p.EsDestacado && p.proId != producto.proId)
                            .ToListAsync();

                        // ...y les quita la marca 'EsDestacado'.
                        foreach (var p in destacadosAnteriores)
                        {
                            p.EsDestacado = false;
                        }
                    }
                    // --- Actualización Final ---

                    // Marca el producto (con todos sus cambios) para Actualizar.
                    _context.Update(producto);

                    // Guarda todos los cambios (el Update del 'producto' y los Updates de los 'destacadosAnteriores').
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                // Manejo de errores de concurrencia (si dos admins editan a la vez).
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.proId)) { return NotFound(); }
                    else { throw; }
                }
            }

            // Si la validación falla, recarga los dropdowns y muestra el formulario de nuevo.
            ViewData["Action"] = "Edit";
            ViewBag.CategoriaId = new SelectList(_context.Categoria, "CategoriaId", "CatNombre", producto.CategoriaId);
            return View(producto);
        }

        /*
         * --- MÉTODO AUXILIAR para Subir Archivos ---
         * Método privado para guardar un archivo IFormFile en el servidor.
         * Recibe el archivo, lo guarda en 'wwwroot/images/drippin' con un nombre único
         * y devuelve la ruta relativa (ej. '~/images/drippin/...') para guardar en la BD.
         * Si el archivo de entrada es nulo, devuelve null.
         */
        private async Task<string> GuardarImagen(IFormFile imagenSubida)
        {
            // Solo procesa si el archivo existe.
            if (imagenSubida != null && imagenSubida.Length > 0)
            {
                // Define la carpeta física de destino.
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "drippin");
                // Crea un nombre de archivo único para evitar colisiones.
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imagenSubida.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Asegura que la carpeta de destino exista
                Directory.CreateDirectory(uploadsFolder);

                // Crea o sobrescribe el archivo en el disco.
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    // Copia los datos del archivo subido al archivo en disco.
                    await imagenSubida.CopyToAsync(fileStream);
                }
                // Devuelve la ruta relativa que se guardará en la BD.
                return "~/images/drippin/" + uniqueFileName;
            }
            return null; // Retorna null si no se subió ningún archivo
        }

        // GET: Productos/Delete/5 - Muestra la página de confirmación de borrado.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Busca el producto (sin incluir categoría, no es necesario).
            var producto = await _context.Producto
                .FirstOrDefaultAsync(m => m.proId == id);
            if (producto == null)
            {
                return NotFound();
            }

            // Muestra la vista de confirmación.
            return View(producto);
        }

        // POST: Productos/Delete/5 - Confirma y ejecuta el borrado.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Busca el producto a borrar por ID.
            var producto = await _context.Producto.FindAsync(id);
            if (producto != null)
            {
                // Lo marca para eliminar.
                _context.Producto.Remove(producto);
            }

            // Ejecuta el borrado en la BD.
            await _context.SaveChangesAsync();
            // Redirige a la lista.
            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar de EF para verificar existencia.
        private bool ProductoExists(int id)
        {
            return _context.Producto.Any(e => e.proId == id);
        }
    }
}