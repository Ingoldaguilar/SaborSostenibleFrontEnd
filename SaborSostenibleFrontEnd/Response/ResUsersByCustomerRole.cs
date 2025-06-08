using Android.Service.Autofill;
using SaborSostenibleFrontEnd.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Response
{
    public class ResUsersByCustomerRole : ResBase
    {
        public List<UserByCustomerRole> Users { get; set; }
    }
}
