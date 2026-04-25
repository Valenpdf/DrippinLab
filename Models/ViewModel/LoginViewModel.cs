using System.ComponentModel.DataAnnotations;

namespace Drippin.Models.ViewModel
{
    /// <summary>
    /// Define el modelo de datos para la captura de credenciales durante el proceso de autenticación de usuarios.
    /// Utilizado principalmente en: <see cref="Controllers.AccesosController"/>.
    /// </summary>
    public class LoginViewModel
    {
        #region Propiedades de Autenticación

        /// <summary>
        /// Dirección de correo electrónico del usuario.
        /// </summary>
        [Required(ErrorMessage = "El campo Correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un formato de correo válido.")]
        [Display(Name = "Correo Electrónico")]
        public string UsCorreo { get; set; }

        /// <summary>
        /// Contraseña de acceso en texto plano para su validación posterior.
        /// </summary>
        [Required(ErrorMessage = "La Contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        /// <summary>
        /// Indica si la sesión debe persistir tras el cierre del navegador.
        /// </summary>
        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }

        #endregion
    }
}
