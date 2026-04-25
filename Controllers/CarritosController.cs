using Drippin.Data;
using Drippin.DTO;
using Drippin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Gestiona las interacciones con el carrito de compras, incluyendo la visualización,
/// adición y eliminación de productos para usuarios autenticados.
/// Retorna vistas en: <see cref="Views.Carritos"/>
/// Utiliza: <see cref="CarritoItem"/>, <see cref="Producto"/>, <see cref="Usuario"/> y <see cref="CarritoItemDTO"/>.
/// </summary>
public class CarritosController : BaseController
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CarritosController"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos inyectado.</param>
    public CarritosController(DrippinContext context) : base(context)
    {
    }

    /// <summary>
    /// Presenta la vista del carrito de compras, listando los productos agregados, cantidades y el cálculo del total general.
    /// Retorna: <see cref="Views.Carritos.Index"/>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Valida que el usuario cuente con una sesión activa.
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Accesos");
        }

        // Recupera el identificador del usuario y realiza la conversión al tipo de dato esperado por el modelo.
        string userIdString = GetCurrentUserId();
        int userIdInt;
        List<CarritoItemDTO> carrito = new List<CarritoItemDTO>();

        if (int.TryParse(userIdString, out userIdInt)) 
        {
            // Ejecuta la consulta para obtener los ítems del carrito integrando la información de productos.
            carrito = await _context.ItemCarrito
                .Where(i => i.IdUsuario == userIdInt)
                .Include(i => i.Producto)
                .Select(i => new CarritoItemDTO
                {
                    proId = i.proId,
                    ProNombre = i.Producto.proNombre,
                    ProImagen = i.Producto.proImagen,
                    ProPrecio = i.Producto.proPrecio,
                    Cantidad = i.Cantidad,
                    Subtotal = i.Producto.proPrecio * i.Cantidad
                })
                .ToListAsync(); 
        }

        // Calcula el monto total acumulado del carrito.
        ViewBag.TotalGeneral = carrito.Sum(i => i.Subtotal);

        return View(carrito);
    }

    /// <summary>
    /// Agrega un producto al carrito de compras del usuario o actualiza su cantidad si ya existe.
    /// </summary>
    /// <param name="proId">Identificador único del producto.</param>
    /// <param name="cantidad">Cantidad de unidades a agregar (valor predeterminado es 1).</param>
    [HttpPost]
    public async Task<IActionResult> AgregarAlCarrito(int proId, int cantidad = 1)
    {
        if (!User.Identity.IsAuthenticated)
        {
            TempData["AlertTitle"] = "¡Necesitas iniciar sesión!";
            TempData["AlertMessage"] = "Tené ingresar a tu cuenta para agregar productos al carrito.";
            TempData["AlertType"] = "warning";
            return RedirectToAction("Login", "Accesos");
        }

        string userIdString = GetCurrentUserId();
        if (!int.TryParse(userIdString, out int userIdInt))
        {
            TempData["Alerta"] = "Error al identificar el usuario (ID no es entero).";
            return RedirectToAction("Index", "Home");
        }

        var producto = await _context.Producto.FirstOrDefaultAsync(p => p.proId == proId);

        if (producto == null)
        {
            TempData["Alerta"] = "Producto no encontrado.";
            return RedirectToAction("Index", "Home");
        }

        cantidad = Math.Max(1, cantidad);

        // Verifica si el producto ya se encuentra registrado en el carrito del usuario.
        var itemDb = await _context.ItemCarrito
            .FirstOrDefaultAsync(i => i.IdUsuario == userIdInt && i.proId == proId);

        if (itemDb != null)
        {
            // Incrementa la cantidad del ítem existente.
            itemDb.Cantidad += cantidad;
            _context.ItemCarrito.Update(itemDb);
            TempData["Alerta"] = $"El producto {producto.proNombre} se actualizó en el carrito. Cantidad actual: {itemDb.Cantidad}.";
        }
        else
        {
            // Crea un nuevo registro en el carrito para el producto y usuario especificados.
            _context.ItemCarrito.Add(new CarritoItem
            {
                IdUsuario = userIdInt,
                proId = producto.proId,
                Cantidad = cantidad,
            });
            TempData["Alerta"] = $"¡{producto.proNombre} agregado al carrito!";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Decrementa la cantidad de un producto en el carrito o lo elimina si la cantidad llega a cero.
    /// </summary>
    /// <param name="proId">Identificador único del producto a remover.</param>
    [HttpPost]
    public async Task<IActionResult> RemoverDelCarrito(int proId) 
    {
        string userIdString = GetCurrentUserId();
        if (!int.TryParse(userIdString, out int userIdInt))
        {
            TempData["Alerta"] = "Error al identificar el usuario (ID no es entero).";
            return RedirectToAction("Index");
        }

        // Recupera el ítem específico del carrito para procesar su remoción o actualización.
        var itemEnCarrito = await _context.ItemCarrito
            .Include(i => i.Producto)
            .FirstOrDefaultAsync(i => i.IdUsuario == userIdInt && i.proId == proId);

        if (itemEnCarrito != null)
        {
            if (itemEnCarrito.Cantidad > 1)
            {
                // Reduce la cantidad en una unidad.
                itemEnCarrito.Cantidad--;
                _context.ItemCarrito.Update(itemEnCarrito);
                TempData["Alerta"] = $"Se restó una unidad de {itemEnCarrito.Producto?.proNombre ?? "producto"}. Cantidad restante: {itemEnCarrito.Cantidad}.";
            }
            else
            {
                // Elimina el ítem del carrito si la cantidad es unitaria.
                _context.ItemCarrito.Remove(itemEnCarrito);
                TempData["Alerta"] = $"Producto {itemEnCarrito.Producto?.proNombre ?? "eliminado"} eliminado del carrito."; 
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}