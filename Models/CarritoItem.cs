using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Drippin.Models
{
    /// <summary>
    /// Representa un elemento individual dentro del carrito de compras de un usuario,
    /// vinculando un producto específico con una cantidad determinada.
    /// Utilizado principalmente en: <see cref="Controllers.CarritosController"/> y <see cref="Controllers.HomeController"/>.
    /// </summary>
    public class CarritoItem
    {
        #region Propiedades de Identidad y Atributos

        /// <summary>
        /// Identificador único del ítem en el carrito.
        /// </summary>
        [Key]
        public int ItemCarritoId { get; set; }

        /// <summary>
        /// Cantidad de unidades del producto seleccionadas por el usuario.
        /// </summary>
        [Required]
        public int Cantidad { get; set; }

        #endregion

        #region Relaciones y Navegación

        /// <summary>
        /// Identificador del usuario propietario del carrito (Clave Foránea).
        /// </summary>
        [Required]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Identificador del producto seleccionado (Clave Foránea).
        /// </summary>
        [Required]
        public int proId { get; set; }

        /// <summary>
        /// Entidad de producto asociada a este ítem.
        /// </summary>
        [ForeignKey("proId")]
        public Producto Producto { get; set; }

        /// <summary>
        /// Entidad de usuario asociada a este ítem.
        /// </summary>
        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; }

        #endregion
    }
}

