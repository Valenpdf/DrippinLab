using System.ComponentModel.DataAnnotations;

namespace Drippin.Models
{
    public class Role
    {
        // Clave Primaria
        [Key]
        
        public int IdRol { get; set; }

        // Nombre del Rol (ej. "Administrador", "Cliente")
        public string NombreRol { get; set; }

        // Propiedad de Navegación para Entity Framework Core
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    }
}
