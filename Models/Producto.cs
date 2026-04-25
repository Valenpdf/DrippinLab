using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drippin.Models
{
    /// <summary>
    /// Representa la entidad de producto en el catálogo, detallando sus atributos comerciales,
    /// recursos multimedia y categorización dentro del sistema.
    /// Utilizado principalmente en: <see cref="Controllers.ProductosController"/>, <see cref="Controllers.HomeController"/>, <see cref="Controllers.CarritosController"/> y <see cref="Controllers.TandasController"/>.
    /// </summary>
    public class Producto
    {
        #region Identificadores y Atributos Básicos

        /// <summary>
        /// Identificador único del producto.
        /// </summary>
        [Key]
        [Display(Name = "ID")]
        public int proId { get; set; }

        /// <summary>
        /// Nombre descriptivo del producto.
        /// </summary>
        [Display(Name = "Nombre de producto")]
        required public string proNombre { get; set; }

        /// <summary>
        /// Valor comercial del producto.
        /// </summary>
        [Range(0.01, 999999)]
        [Precision(18, 2)]
        [Display(Name = "Precio")]
        required public decimal proPrecio { get; set; }

        /// <summary>
        /// Descripción técnica o comercial detallada del producto.
        /// </summary>
        [Display(Name = "Descripción")]
        public string? proDescripcion { get; set; }

        /// <summary>
        /// Especificaciones de dimensiones o medidas del producto.
        /// </summary>
        [Display(Name = "Medidas")]
        public string? proMedidas { get; set; }

        #endregion

        #region Estado y Metadatos

        /// <summary>
        /// Indica si el producto se encuentra disponible para su visualización y venta.
        /// </summary>
        [Display(Name = "Disponible")]
        public bool Disponible { get; set; }

        /// <summary>
        /// Fecha y hora en la que el registro del producto fue creado.
        /// </summary>
        [Display(Name = "Fecha de creación")]
        public DateTime prodCreacion { get; set; } = DateTime.Now;

        /// <summary>
        /// Determina si el producto posee una marca de destaque en la interfaz principal.
        /// </summary>
        [Display(Name = "Es destacado")]
        public bool EsDestacado {  get; set; }

        #endregion

        #region Recursos Multimedia

        /// <summary>
        /// Ruta o referencia de la imagen principal del producto.
        /// </summary>
        [Display(Name = "Imagen")]
        public string? proImagen { get; set; }

        /// <summary>
        /// Ruta o referencia de la imagen 2 del producto.
        /// </summary>
        [Display(Name = "Imagen 2")]
        public string? proImagen2 { get; set; }

        /// <summary>
        /// Ruta o referencia de la imagen 3 del producto.
        /// </summary>
        [Display(Name = "Imagen 3")]
        public string? proImagen3 { get; set; }

        /// <summary>
        /// Ruta o referencia la imagen 4 del producto.
        /// </summary>
        [Display(Name = "Imagen 4")]
        public string? proImagen4 { get; set; }

        #endregion

        #region Relaciones y Navegación

        /// <summary>
        /// Identificador de la categoría asociada (Clave Foránea).
        /// </summary>
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        /// <summary>
        /// Entidad de categoría asociada al producto.
        /// </summary>
        public Categoria? Categoria { get; set; }

        /// <summary>
        /// Identificador de la tanda o colección asociada (Clave Foránea).
        /// </summary>
        public int? proTandaId { get; set; }

        /// <summary>
        /// Entidad de tanda asociada al producto.
        /// </summary>
        [ForeignKey("proTandaId")]
        public virtual Tanda? Tanda { get; set; }

        #endregion
    }
}
