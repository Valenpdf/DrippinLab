using System.ComponentModel.DataAnnotations;

namespace Drippin.Models
{
    /// <summary>
    /// Representa una categoría de productos dentro del sistema, permitiendo la agrupación 
    /// lógica y la navegación jerárquica en el catálogo.
    /// Utilizado principalmente en: <see cref="Controllers.CategoriasController"/>, <see cref="Controllers.ProductosController"/> y <see cref="Controllers.HomeController"/>.
    /// </summary>
    public class Categoria
    {
        #region Propiedades de Identidad y Atributos

        /// <summary>
        /// Identificador único de la categoría.
        /// </summary>
        [Key]
        public int CategoriaId { get; set; }

        /// <summary>
        /// Nombre descriptivo de la categoría.
        /// </summary>
        [Required]
        [Display(Name = "Nombre de la Categoría")]
        public string CatNombre { get; set; }

        /// <summary>
        /// Identificador o ruta del icono representativo de la categoría.
        /// </summary>
        public string? CatICO { get; set; }

        #endregion

        #region Relaciones y Navegación

        /// <summary>
        /// Colección de productos asociados a esta categoría.
        /// </summary>
        public ICollection<Producto>? Productos { get; set; }

        #endregion
    }
}
