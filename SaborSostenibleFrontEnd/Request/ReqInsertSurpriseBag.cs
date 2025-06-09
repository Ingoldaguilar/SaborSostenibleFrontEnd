using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Request
{
    public class ReqInsertSurpriseBag
    {
        public bool IsDonation { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int FoodBankId { get; set; }
    }
}
