using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Entities
{
    public class PendingDonation
    {
        public string BagDescription { get; set; }
        public string BusinessName { get; set; }
        public string BusinessPhone { get; set; }
        public string BusinessAddress { get; set; }
        public int BagId { get; set; }
        public string FoodBankName { get; set; }
        public string FoodBankPhone { get; set; }
        public string FoodBankAddress { get; set; }
    }
}
