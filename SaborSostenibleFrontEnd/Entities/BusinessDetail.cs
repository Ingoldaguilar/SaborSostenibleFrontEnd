using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Entities
{
    public class BusinessDetail
    {
        public int BusinessId { get; set; }
        public string LogoImage { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string PhoneNumber { get; set; }
    }
}
