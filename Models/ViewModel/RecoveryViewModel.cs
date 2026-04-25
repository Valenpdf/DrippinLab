using System.ComponentModel.DataAnnotations;

namespace Drippin.Models.ViewModel
{
    /// <summary>
    /// Define el modelo de datos para la solicitud inicial de recuperación de cuenta
    /// mediante el suministro de una dirección de correo electrónico.
    /// Utilizado principalmente en: <see cref="Controllers.AccesosController"/>.
    /// </summary>
    public class RecoveryViewModel
    {
        #region Propiedades de Identidad

        /// <summary>
        /// Dirección de correo electrónico asociada a la cuenta que se desea recuperar.
        /// </summary>
        [EmailAddress]
        [Required(ErrorMessage = "Campo requerido")]
        public string UsCorreo { get; set; }

        #endregion
    }
}
