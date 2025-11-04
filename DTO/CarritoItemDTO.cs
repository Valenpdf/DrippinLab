namespace Drippin.DTO
{
    public class CarritoItemDTO
    {
        public int proId { get; set; }
        public string ProNombre { get; set; }
        public string ProImagen { get; set; } // Opcional: para mostrar la imagen en el carrito
        public decimal ProPrecio { get; set; }

        // Propiedad calculada para el subtotal
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }
}
