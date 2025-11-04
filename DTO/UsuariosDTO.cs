using System.ComponentModel.DataAnnotations;


namespace Drippin.DTO
{
    public class UsuariosDTO
    {

        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El Nombre debe tener un máximo de {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]*$", ErrorMessage = "El Nombre solo debe contener letras, espacios, guiones o apóstrofes.")]
        [Display(Name = "Nombre")]
        public string UsNombre { get; set; }

        [Required(ErrorMessage = "El Apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El Apellido debe tener un máximo de {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]*$", ErrorMessage = "El Nombre solo debe contener letras, espacios, guiones o apóstrofes.")]
        [Display(Name = "Apellido")]
        public string UsApellido { get; set; }


        [Required(ErrorMessage = "El campo Correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un formato de correo válido.")]
        [StringLength(100, ErrorMessage = "El Correo debe tener un máximo de {1} caracteres.")]
        [Display(Name = "Correo Electrónico")]
        public string UsCorreo { get; set; }


        [Required(ErrorMessage = "La Contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "La Contraseña debe tener entre {2} y {1} caracteres.")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }


        [Required(ErrorMessage = "Debe confirmar la Contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La Contraseña y su Confirmación no coinciden.")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmarPassword { get; set; }

    }
}
