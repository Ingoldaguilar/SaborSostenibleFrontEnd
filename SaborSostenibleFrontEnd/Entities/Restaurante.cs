using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Entities
{
    public class Restaurante
    {
        public int idRestaurante { get; set; }
        public string nombreRestaurante { get; set; }
        public string descripcionRestaurante { get; set; }
        public string imagen { get; set; }
        public string telefono { get; set; }
    }
}
