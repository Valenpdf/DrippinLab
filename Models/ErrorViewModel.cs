namespace Drippin.Models
{
    /// <summary>
    /// Define el modelo de datos para la presentación de errores capturados en el sistema,
    /// facilitando el diagnóstico mediante identificadores de solicitud.
    /// </summary>
    public class ErrorViewModel
    {
        #region Propiedades de Diagnóstico

        /// <summary>
        /// Identificador único de la solicitud que originó el error.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Indica si el identificador de solicitud debe ser visualizado en la interfaz.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        #endregion
    }
}
