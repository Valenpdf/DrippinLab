namespace Drippin.DTO
{
    /// <summary>
    /// Objeto de Transferencia de Datos (DTO) para representar la información de un ítem en el carrito,
    /// optimizado para su visualización en la interfaz de usuario.
    /// Utilizado en: <see cref="Controllers.CarritosController"/>.
    /// </summary>
    public class CarritoItemDTO
    {
        #region Propiedades

        /// <summary>
        /// Identificador único del producto.
        /// </summary>
        public int proId { get; set; }

        /// <summary>
        /// Nombre descriptivo del producto.
        /// </summary>
        public string ProNombre { get; set; }

        /// <summary>
        /// Ruta o referencia del recurso visual del producto para su visualización en el carrito.
        /// </summary>
        public string ProImagen { get; set; }

        /// <summary>
        /// Precio unitario del producto al momento de la consulta.
        /// </summary>
        public decimal ProPrecio { get; set; }

        /// <summary>
        /// Cantidad de unidades seleccionadas.
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Monto calculado resultante de multiplicar el precio unitario por la cantidad.
        /// </summary>
        public decimal Subtotal { get; set; }

        #endregion
    }
}
