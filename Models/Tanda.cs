using System.ComponentModel.DataAnnotations;

namespace Drippin.Models
{
    /// <summary>
    /// Representa una Tanda o colección temporal de productos, permitiendo gestionar su 
    /// visibilidad conjunta y periodos de vigencia dentro del sistema.
    /// Utilizado principalmente en: <see cref="Controllers.TandasController"/> y <see cref="Controllers.HomeController"/>.
    /// </summary>
    public class Tanda
    {
        #region Propiedades de Identidad y Atributos

        /// <summary>
        /// Identificador único de la tanda.
        /// </summary>
        [Key]
        public int TandaId { get; set; }

        /// <summary>
        /// Nombre descriptivo de la tanda.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre de la Tanda")]
        public string TanNombre { get; set; }

        /// <summary>
        /// Indica si la tanda debe visualizarse de forma prominente en la interfaz principal.
        /// </summary>
        [Display(Name = "Mostrar en el Index")]
        public bool TanVisible { get; set; } = true;

        #endregion

        #region Vigencia y Temporalidad

        /// <summary>
        /// Fecha y hora de inicio de la vigencia de la tanda.
        /// </summary>
        public DateTime? TanFechaInicio { get; set; }

        /// <summary>
        /// Fecha y hora de finalización de la vigencia de la tanda.
        /// </summary>
        public DateTime? TanFechaFin { get; set; }

        #endregion

        #region Relaciones y Navegación

        /// <summary>
        /// Colección de productos asociados a esta tanda específica.
        /// </summary>
        public virtual ICollection<Producto>? Productos { get; set; }

        #endregion
    }
}