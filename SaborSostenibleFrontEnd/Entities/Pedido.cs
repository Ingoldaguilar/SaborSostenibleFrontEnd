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
    }
}
