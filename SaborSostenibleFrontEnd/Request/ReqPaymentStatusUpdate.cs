using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Request
{
    public class ReqPaymentStatusUpdate
    {
        public int OrderId { get; set; }
        public int StateCode { get; set; }
    }
}
