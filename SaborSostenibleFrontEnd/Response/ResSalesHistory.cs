using SaborSostenibleFrontEnd.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Response
{
    public class ResSalesHistory : ResBase
    {
        public List<SalesHistoryItem> SalesHistory { get; set; }
    }
}
