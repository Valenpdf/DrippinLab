using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Drippin.Models
{
    public class CarritoItem
    {
        // Clave primaria
        [Key]
        public int ItemCarritoId { get; set; }

        // Clave foránea al usuario autenticado (se recomienda usar string si usas ASP.NET Identity)
        // Debes ajustar el tipo de dato (string/int) según el tipo de tu clave de usuario.
        [Required]
        public int IdUsuario { get; set; }

        // Clave foránea al producto
        [Required]
        public int proId { get; set; }

        // Cantidad del producto
        [Required]
        public int Cantidad { get; set; }

        // Propiedad de navegación a la tabla de Producto
        [ForeignKey("proId")]
        public Producto Producto { get; set; }

        // Propiedad de navegación a la tabla de Usuario
        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } 
    }
}

