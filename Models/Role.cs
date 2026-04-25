using System.ComponentModel.DataAnnotations;

namespace Drippin.Models
{
    /// <summary>
    /// Representa un rol de usuario dentro del sistema de seguridad y control de acceso,
    /// permitiendo definir permisos y niveles de autorización.
    /// Utilizado principalmente en: <see cref="Controllers.RolesController"/>, <see cref="Controllers.UsuariosController"/> y <see cref="Controllers.AccesosController"/>.
    /// </summary>
    public class Role
    {
        #region Propiedades de Identidad y Atributos

        /// <summary>
        /// Identificador único del rol.
        /// </summary>
        [Key]
        public int IdRol { get; set; }

        /// <summary>
        /// Nombre descriptivo del rol (ej: Administrador, Cliente).
        /// </summary>
        public string NombreRol { get; set; }

        #endregion

        #region Relaciones y Navegación

        /// <summary>
        /// Colección de usuarios asociados a este rol.
        /// </summary>
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

        #endregion
    }
}
