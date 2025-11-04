using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Drippin.Models
{
    public class Producto
    {

        [Key]
        [Display(Name = "ID")]
        public int proId { get; set; }

        [Display(Name = "Nombre de producto")]
        required public string proNombre { get; set; }

        [Range(0.01, 999999)]
        [Precision(18, 2)]
        [Display(Name = "Precio")]
        required public decimal proPrecio { get; set; }
        
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }   // FK

        [Display(Name = "Descripción")]
        public string? proDescripcion { get; set; }

        [Display(Name = "Medidas")]
        public string? proMedidas { get; set; }

        public Categoria? Categoria { get; set; }  // Propiedad de navegación

        [Display(Name = "Imagen")]
        public string? proImagen { get; set; }

        [Display(Name = "Imagen 2")]
        public string? proImagen2 { get; set; }

        [Display(Name = "Imagen 3")]
        public string? proImagen3 { get; set; }

        [Display(Name = "Imagen 4")]
        public string? proImagen4 { get; set; }

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; }

        [Display(Name = "Fecha de creación")]
        public DateTime prodCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Es destacado")]
        public bool EsDestacado {  get; set; }

    }
}
