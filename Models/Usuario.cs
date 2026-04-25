using Blazorise;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drippin.Models
{
    /// <summary>
    /// Representa la entidad de usuario en el sistema, conteniendo sus atributos de identidad,
    /// credenciales de acceso y relaciones con roles y carrito de compras.
    /// Utilizado principalmente en: <see cref="Controllers.AccesosController"/>, <see cref="Controllers.UsuariosController"/> y <see cref="Controllers.InicioController"/>.
    /// </summary>
    public class Usuario
    {
        #region Propiedades de Identidad y Datos Personales

        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        [Key]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [Required(ErrorMessage = "El Nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El Nombre debe tener un máximo de {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]*$", ErrorMessage = "El Nombre solo debe contener letras, espacios, guiones o apóstrofes.")]
        [Display(Name = "Nombre")]
        public string UsNombre { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        [Required(ErrorMessage = "El Apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El Apellido debe tener un máximo de {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]*$", ErrorMessage = "El Apellido solo debe contener letras, espacios, guiones o apóstrofes.")]
        [Display(Name = "Apellido")]
        public string UsApellido { get; set; }

        /// <summary>
        /// Dirección de correo electrónico asociada a la cuenta.
        /// </summary>
        [Required(ErrorMessage = "El campo Correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un formato de correo válido.")]
        [StringLength(100, ErrorMessage = "El Correo debe tener un máximo de {1} caracteres.")]
        [Display(Name = "Correo Electrónico")]
        public string UsCorreo { get; set; }

        #endregion

        #region Credenciales y Seguridad

        /// <summary>
        /// Almacena el hash de la contraseña del usuario para la validación de acceso.
        /// </summary>
        [Required(ErrorMessage = "La Contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "La Contraseña debe tener entre {2} y {1} caracteres.")]
        [Display(Name = "Contraseña")]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Token temporal para procesos de recuperación de contraseña.
        /// </summary>
        public string? Token { get; set; } = "tokenbloqueado";

        /// <summary>
        /// Fecha y hora de expiración del token de recuperación.
        /// </summary>
        public DateTime? FechaExpiracionToken { get; set; }

        /// <summary>
        /// Fecha y hora en la que el usuario se registró en el sistema.
        /// </summary>
        public DateTime FechaRegistro { get; set; }

        #endregion

        #region Relaciones y Navegación

        /// <summary>
        /// Identificador del rol asignado al usuario (Clave Foránea).
        /// </summary>
        [Required(ErrorMessage = "Debe seleccionar un Rol.")]
        public int IdRol { get; set; }

        /// <summary>
        /// Entidad de rol asociada al usuario.
        /// </summary>
        [ValidateNever]
        public virtual Role? Role { get; set; }

        /// <summary>
        /// Colección de ítems contenidos en el carrito de compras del usuario.
        /// </summary>
        public virtual ICollection<CarritoItem> CarritoItems { get; set; } = new List<CarritoItem>();

        #endregion
    }
}
