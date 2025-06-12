using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Entities
{
    public class PendingSaleDetail
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public decimal TotalAmount { get; set; }
        public int StateCode { get; set; }

        public string StateText =>
            StateCode switch
            {
                1 => "Disponible",
                2 => "Vendida",
                3 => "Donada",
                4 => "Expirada",
                5 => "Pendiente",
                6 => "Completado",
                7 => "Cancelado",
                8 => "Recogido",
                9 => "Entregado",
                10 => "Denegado",
                _ => "Desconocido"
            };
    }
}
