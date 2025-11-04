using System.ComponentModel.DataAnnotations;

namespace Drippin.Models
{
    public class Categoria
    {
        [Key]
        public int CategoriaId { get; set; }

        [Required]
        [Display(Name = "Nombre de la Categoría")]
        public string CatNombre { get; set; }

        public string? CatICO { get; set; }

        // Relación uno a muchos con Producto
        public ICollection<Producto>? Productos { get; set; }
    }
}
