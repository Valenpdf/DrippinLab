using System.ComponentModel.DataAnnotations;

namespace Drippin.Models.ViewModel
{
    public class LoginViewModel
    {
        // --- Correo (UsCorreo) ---
        [Required(ErrorMessage = "El campo Correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un formato de correo válido.")]
        [Display(Name = "Correo Electrónico")]
        public string UsCorreo { get; set; }

        // --- Contraseña ---
        [Required(ErrorMessage = "La Contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        // --- Recordarme ---
        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }

    }
}
