namespace Drippin.Models
{
    /// <summary>
    /// Provee métodos auxiliares para el procesamiento y formateo de rutas de recursos visuales
    /// dentro del sistema, asegurando la resolución correcta de URLs.
    /// </summary>
    public static class ImageHelper
    {
        #region Métodos de Formateo

        /// <summary>
        /// Resuelve y normaliza la ruta de una imagen basándose en los datos persistidos en la base de datos.
        /// </summary>
        /// <param name="rutaBD">Ruta original almacenada en el registro de la base de datos.</param>
        /// <returns>Ruta formateada y lista para su consumo en la interfaz de usuario.</returns>
        public static string FormatearRuta(string? rutaBD)
        {
            // Retorna una imagen por defecto en caso de ausencia de ruta definida.
            if (string.IsNullOrEmpty(rutaBD)) return "~/images/ejemplo1.jpg";

            // Valida si la ruta ya posee un formato absoluto o relativo a la raíz.
            if (rutaBD.StartsWith("~") || rutaBD.StartsWith("http") || rutaBD.StartsWith("/")) return rutaBD;

            // Concatena el prefijo de almacenamiento estándar para imágenes del catálogo.
            return "~/images/drippin/" + rutaBD;
        }

        #endregion
    }
}
