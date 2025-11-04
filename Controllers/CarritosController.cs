using Drippin.Data;
using Drippin.DTO;
using Drippin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/* Este controlador gestiona todas las interacciones con el carrito de compras del usuario. 
 * Es fundamental que el usuario esté autenticado para usarlo.*/
public class CarritosController : BaseController
{
    // Constructor: Recibe DrippinContext y lo pasa a la clase base
    public CarritosController(DrippinContext context) : base(context)
    {
    }

    // Carritos/Index - Muestra la página del carrito (lista de productos, cantidades y total)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        /* Verifica si el usuario está autenticado. */
        if (!User.Identity.IsAuthenticated)
        {
            /* Si no está autenticado, redirige al login. */
            return RedirectToAction("Login", "Accesos");
        }

        /* Obtiene el ID del usuario actual con GetCurrentUserId como string (Las Claims se guardan como String) */
        string userIdString = GetCurrentUserId();
        int userIdInt; // Esto para pasar el userId a int

        /* Declara e inicializa 'carrito' afuera del bloque 'if' */
        List<CarritoItemDTO> carrito = new List<CarritoItemDTO>();

        /* Se intenta pasar la claim ID del usuario de String a Int ya que tiene que ser si o si un entero */
        if (int.TryParse(userIdString, out userIdInt)) 
        {
            // Consulta la BD: Obtiene los ítems y hace JOIN con Producto
            carrito = await _context.ItemCarrito
                .Where(i => i.IdUsuario == userIdInt) // Filtra solo los articulos de ESE usuario (Int)
                .Include(i => i.Producto)             // Trae la info del producto (nombre, precio, cantidad e imagen)                
                .Select(i => new CarritoItemDTO
                {
                    proId = i.proId,
                    ProNombre = i.Producto.proNombre,
                    ProImagen = i.Producto.proImagen,
                    ProPrecio = i.Producto.proPrecio,
                    Cantidad = i.Cantidad,
                    Subtotal = i.Producto.proPrecio * i.Cantidad
                })

                /* Proyecta los resultados en la lista CarritoItemDTO, que se encarga de mostrar
                 * los productos agregados al carrito y calcular el subtotal de cada linea */
                .ToListAsync(); 
        }
        // Si el 'if' no se cumple, 'carrito' se mantiene como una lista vacía

        // Suma todos los subtotales 
        ViewBag.TotalGeneral = carrito.Sum(i => i.Subtotal);


        /* Devuelve la vista, pasándole la lista de CarritoItemDTO como modelo */
        return View(carrito);
    }

    // ---------------------------------------------------------------- //
    // AGREGAR AL CARRITO 
    // ---------------------------------------------------------------- //

    /* Recibe el ID del producto (proId) y la cantidad (1 como default) */
    [HttpPost]
    public async Task<IActionResult> AgregarAlCarrito(int proId, int cantidad = 1)
    {
        /* VERIFICACIÓN DE AUTENTICACIÓN */
        if (!User.Identity.IsAuthenticated)
        {
            TempData["AlertTitle"] = "¡Necesitas iniciar sesión!";
            TempData["AlertMessage"] = "Debes ingresar a tu cuenta para agregar productos al carrito.";
            TempData["AlertType"] = "warning";
            return RedirectToAction("Login", "Accesos");
        }

        /* Obtener el ID del usuario y lo pasa a entero */
        string userIdString = GetCurrentUserId();
        if (!int.TryParse(userIdString, out int userIdInt))
        {
            TempData["Alerta"] = "Error al identificar el usuario (ID no es entero).";
            return RedirectToAction("Index", "Home");
        }

        /* Obtiene el producto de la BD (usando proId) */
        var producto = await _context.Producto.FirstOrDefaultAsync(p => p.proId == proId);

        if (producto == null)
        {
            TempData["Alerta"] = "Producto no encontrado.";
            return RedirectToAction("Index", "Home");
        }

        /* Asegurar que la cantidad sea al menos 1 */
        cantidad = Math.Max(1, cantidad);

        /* Verifica si el producto YA está en el carrito de la DB para ESTE usuario */
        var itemDb = await _context.ItemCarrito
            .FirstOrDefaultAsync(i => i.IdUsuario == userIdInt && i.proId == proId);

        if (itemDb != null)
        {
            /* Si el producto ya está, actualizar Cantidad en la BD */
            itemDb.Cantidad += cantidad;
            _context.ItemCarrito.Update(itemDb); /* Marca la entidad como modificada */
            TempData["Alerta"] = $"El producto {producto.proNombre} se actualizó en el carrito. Cantidad actual: {itemDb.Cantidad}.";
        }
        else
        {
            /* Si no existe, crear y añadir un nuevo item a la DB */
            _context.ItemCarrito.Add(new CarritoItem
            {
                IdUsuario = userIdInt,  /* Asigna el ID del usuario */
                proId = producto.proId, /* Asigna el ID del produco*/
                Cantidad = cantidad,    /* Asigna la cantidad del producto */
            });
            TempData["Alerta"] = $"¡{producto.proNombre} agregado al carrito!";
        }

        /* Guardar los cambios en la BD */
        await _context.SaveChangesAsync();

        return RedirectToAction("Index"); /* Redirige a la vista del carrito */
    }

    // ---------------------------------------------------------------- //
    // REMOVER DEL CARRITO
    // ---------------------------------------------------------------- //

    /* Recibe el ID del producto (proId) */
    [HttpPost]
    public async Task<IActionResult> RemoverDelCarrito(int proId) 
    {
        /* Obtiene y pasa a entero la ID del usuario */
        string userIdString = GetCurrentUserId();
        if (!int.TryParse(userIdString, out int userIdInt))
        {
            TempData["Alerta"] = "Error al identificar el usuario (ID no es entero).";
            return RedirectToAction("Index");
        }

        /* Buscar el item a remover en la DB para ESTE usuario
         * Incluye Producto para obtener el nombre para el TempData */
        var itemEnCarrito = await _context.ItemCarrito
            .Include(i => i.Producto)
            .FirstOrDefaultAsync(i => i.IdUsuario == userIdInt && i.proId == proId);

        if (itemEnCarrito != null)
        {
            if (itemEnCarrito.Cantidad > 1)
            {
                /* Si hay más de 1, solo resta la cantidad */
                itemEnCarrito.Cantidad--; // Resta 1 
                _context.ItemCarrito.Update(itemEnCarrito); // Marca la entidad como modificada

                
                TempData["Alerta"] = $"Se restó una unidad de {itemEnCarrito.Producto?.proNombre ?? "producto"}. Cantidad restante: {itemEnCarrito.Cantidad}.";
            }
            else
            {
                /* Si la cantidad es 1, elimina el item */
                _context.ItemCarrito.Remove(itemEnCarrito);
                TempData["Alerta"] = $"Producto {itemEnCarrito.Producto?.proNombre ?? "eliminado"} eliminado del carrito."; 
            }

            /* Guardar los cambios (sea un Update o un Remove) */
            await _context.SaveChangesAsync(); // 
        }

        return RedirectToAction("Index");
    }
}