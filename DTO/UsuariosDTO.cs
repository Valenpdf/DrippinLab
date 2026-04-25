using System.ComponentModel.DataAnnotations;


namespace Drippin.DTO
{
    /// <summary>
    /// Objeto de Transferencia de Datos (DTO) para la gestión de registros y perfiles de usuario,
    /// incluyendo validaciones de integridad y seguridad.
    /// Utilizado en: <see cref="Controllers.AccesosController"/>.
    /// </summary>
    public class UsuariosDTO
    {
        #region Propiedades de Identidad

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El Nombre debe tener un máximo de {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]*$", ErrorMessage = "El Nombre solo debe contener letras, espacios, guiones o apóstrofes.")]
        [Display(Name = "Nombre")]
        public string UsNombre { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        [Required(ErrorMessage = "El Apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El Apellido debe tener un máximo de {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]*$", ErrorMessage = "El Nombre solo debe contener letras, espacios, guiones o apóstrofes.")]
        [Display(Name = "Apellido")]
        public string UsApellido { get; set; }

        /// <summary>
        /// Dirección de correo electrónico (utilizado como identificador de acceso).
        /// </summary>
        [Required(ErrorMessage = "El campo Correo es obligatorio.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El formato del correo no es válido (ej: usuario@dominio.com)")]
        [StringLength(100, ErrorMessage = "El Correo debe tener un máximo de {1} caracteres.")]
        [Display(Name = "Correo Electrónico")]
        public string UsCorreo { get; set; }

        #endregion

        #region Seguridad y Contraseñas

        /// <summary>
        /// Contraseña de acceso elegida por el usuario.
        /// </summary>
        [Required(ErrorMessage = "La Contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "La Contraseña debe tener entre {2} y {1} caracteres.")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        /// <summary>
        /// Campo de validación para asegurar la coincidencia de la contraseña ingresada.
        /// </summary>
        [Required(ErrorMessage = "Debe confirmar la Contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La Contraseña y su Confirmación no coinciden.")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmarPassword { get; set; }

        #endregion
    }
}
