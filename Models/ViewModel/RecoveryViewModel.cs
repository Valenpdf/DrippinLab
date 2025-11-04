using System.ComponentModel.DataAnnotations;

namespace Drippin.Models.ViewModel
{
    public class RecoveryViewModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "Campo requerido")]
        public string UsCorreo { get; set; }
    }
}
