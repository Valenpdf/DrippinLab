using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drippin.Models
{
    public class Acceso
    {
        // Clave Primaria
        [Key]
        public int IdAcceso { get; set; }

        // Referencia al Usuario
        // Puede ser nullable si el intento de login fue fallido (ej. usuario no existe)
        public int? IdUsuario { get; set; }

        // Propiedad de navegación (Foreign Key)
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        // Detalles del Intento de Acceso
        public DateTime FechaAcceso { get; set; }

        // IP de donde se conectó el usuario
        [StringLength(50)]
        public string IPAddress { get; set; }

        // Si el intento fue exitoso (true) o fallido (false)
        public bool Exitoso { get; set; }

        // Información adicional sobre fallas (ej: "Contraseña incorrecta", "Usuario bloqueado")
        [StringLength(255)]
        public string? DetalleFalla { get; set; }

        // Identificador del navegador/dispositivo
        [StringLength(500)]
        public string? UserAgent { get; set; }

    }
}
