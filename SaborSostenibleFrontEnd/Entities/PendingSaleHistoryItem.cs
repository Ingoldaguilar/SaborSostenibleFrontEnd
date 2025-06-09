using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Entities
{
    public class PendingSaleHistoryItem
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string LogoImage { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int StateCode { get; set; }

        public string StateText =>
            StateCode switch
            {
                5 => "Pendiente",
                _ => "Desconocido"
            };
    }
}
