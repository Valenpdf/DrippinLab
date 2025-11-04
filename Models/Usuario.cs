using Blazorise;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drippin.Models
{
    public class Usuario
    {
        
        [Key]
        public int IdUsuario { get; set; }

        // Datos del Usuario
        [Required(ErrorMessage = "El Nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El Nombre debe tener un máximo de {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]*$", ErrorMessage = "El Nombre solo debe contener letras, espacios, guiones o apóstrofes.")]
        [Display(Name = "Nombre")]
        public string UsNombre { get; set; }

        [Required(ErrorMessage = "El Apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El Apellido debe tener un máximo de {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]*$", ErrorMessage = "El Apellido solo debe contener letras, espacios, guiones o apóstrofes.")]
        [Display(Name = "Apellido")]
        public string UsApellido { get; set; }

        [Required(ErrorMessage = "El campo Correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un formato de correo válido.")]
        [StringLength(100, ErrorMessage = "El Correo debe tener un máximo de {1} caracteres.")]
        [Display(Name = "Correo Electrónico")]
        public string UsCorreo { get; set; }

        // Debe contener el hash y el salt (ej. del BCrypt)
        [Required(ErrorMessage = "La Contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "La Contraseña debe tener entre {2} y {1} caracteres.")]
        [Display(Name = "Contraseña")]
        public string PasswordHash { get; set; }

        // Relación con Roles (Foreign Key)
        [Required(ErrorMessage = "Debe seleccionar un Rol.")]
        public int IdRol { get; set; }

        
        public virtual Role? Role { get; set; } // Propiedad de navegación

        public DateTime FechaRegistro { get; set; }

        // Campos de Recuperación de Contraseña
        public string? Token { get; set; } = "tokenbloqueado"; // El ? indica que es anulable (nullable)
        public DateTime? FechaExpiracionToken { get; set; }

        public virtual ICollection<CarritoItem> CarritoItems { get; set; } = new List<CarritoItem>();

    }
}
