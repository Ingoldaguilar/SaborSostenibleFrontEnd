using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Request
{
    public class ReqFoodBankIsActiveUpdate
    {
        public int FoodBankId { get; set; }
        public bool IsActive { get; set; }
    }
}
