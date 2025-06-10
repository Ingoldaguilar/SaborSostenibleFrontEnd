using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Entities
{
    public class SalesHistoryItem
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string LogoImage { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int StateCode { get; set; }

        //Mapear a texto
        public string StateText =>
            StateCode switch
            {
                6 => "Completado",
                10 => "Denegado",
                // otros códigos...
                _ => "Desconocido"
            };
    }
}
