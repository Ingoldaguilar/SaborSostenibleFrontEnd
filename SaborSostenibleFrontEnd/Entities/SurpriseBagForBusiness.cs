using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Entities
{
    public class SurpriseBagForBusiness
    {
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public string State { get; set; }
    }
}
