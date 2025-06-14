namespace SaborSostenibleFrontEnd.Entities
{
    public class Pedido
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string RestaurantName { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }  // Formato: "dd/MM/yyyy HH:mm"
        public decimal Total { get; set; } // Monto total en colones ₡
        public int Bags { get; set; } // Cantidad de bolsas

        public string StatusColor
        {
            get
            {
                return Status?.ToLower() switch
                {
                    "pendiente" => "#FF9800", // Naranja
                    "aceptado" => "#4CAF50",  // Verde
                    "denegado" => "#F44336",  // Rojo
                    "completado" => "#2196F3", // Azul
                    _ => "#757575" // Gris por defecto
                };
            }
        }

        public string DateFormatted
        {
            get
            {
                if (DateTime.TryParse(Date, out DateTime parsedDate))
                {
                    return parsedDate.ToString("dd/MM/yyyy HH:mm");
                }
                return Date; // Si no se puede parsear, devuelve el original
            }
        }
    }
}
