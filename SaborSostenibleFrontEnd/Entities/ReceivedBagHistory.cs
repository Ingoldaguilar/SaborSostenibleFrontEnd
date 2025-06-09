using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Entities
{
    public class ReceivedBagHistory
    {
        public string BagDescription { get; set; }
        public string State { get; set; }
        public DateTime DonationDate { get; set; }
        public string BusinessName { get; set; }
        public string LogoUrl { get; set; }
    }
}
