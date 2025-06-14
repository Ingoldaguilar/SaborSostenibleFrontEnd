using SaborSostenibleFrontEnd.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Response
{
    public class ResFoodBankNameAndEmail : ResBase
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
