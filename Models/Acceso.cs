using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drippin.Models
{
    /// <summary>
    /// Representa un registro de intento de acceso al sistema, permitiendo el seguimiento
    /// de auditoría, seguridad y análisis de actividad de los usuarios.
    /// </summary>
    public class Acceso
    {
        #region Propiedades de Identidad

        /// <summary>
        /// Identificador único del registro de acceso.
        /// </summary>
        [Key]
        public int IdAcceso { get; set; }

        /// <summary>
        /// Identificador del usuario asociado al intento de acceso (Opcional).
        /// </summary>
        public int? IdUsuario { get; set; }

        #endregion

        #region Detalles del Evento de Acceso

        /// <summary>
        /// Fecha y hora exacta en la que se produjo el intento de acceso.
        /// </summary>
        public DateTime FechaAcceso { get; set; }

        /// <summary>
        /// Dirección IP desde la cual se originó la solicitud de acceso.
        /// </summary>
        [StringLength(50)]
        public string IPAddress { get; set; }

        /// <summary>
        /// Indica si el intento de autenticación resultó exitoso.
        /// </summary>
        public bool Exitoso { get; set; }

        /// <summary>
        /// Descripción técnica de la causa del fallo de acceso, en caso de que corresponda.
        /// </summary>
        [StringLength(255)]
        public string? DetalleFalla { get; set; }

        /// <summary>
        /// Información sobre el agente de usuario (navegador o dispositivo) utilizado.
        /// </summary>
        [StringLength(500)]
        public string? UserAgent { get; set; }

        #endregion

        #region Relaciones y Navegación

        /// <summary>
        /// Entidad de usuario asociada al registro de acceso.
        /// </summary>
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        #endregion
    }
}
