using System.ComponentModel.DataAnnotations;

namespace Drippin.Models.ViewModel
{
    /// <summary>
    /// Define el modelo de datos necesario para el establecimiento de una nueva contraseña 
    /// durante el flujo de recuperación de cuenta.
    /// </summary>
    public class RecoveryPasswordViewModel
    {
        #region Propiedades de Seguridad

        /// <summary>
        /// Token de validación que vincula la solicitud con la identidad del usuario.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Nueva contraseña definida por el usuario.
        /// </summary>
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "La Contraseña debe tener entre {2} y {1} caracteres.")]
        [Display(Name = "Nueva Contraseña")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Confirmación de la nueva contraseña para asegurar la integridad del cambio.
        /// </summary>
        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("NewPassword", ErrorMessage = "La contraseña y su confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }

        #endregion
    }
}
