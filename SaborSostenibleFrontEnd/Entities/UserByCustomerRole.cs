using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Entities
{
    public class UserByCustomerRole
    {
        public int UserId { get; set; }
        public string FirstName1 { get; set; }
        public string FirstName2 { get; set; }
        public string LastName1 { get; set; }
        public string LastName2 { get; set; }

        public string FullName =>
            $"{FirstName1} {FirstName2} {LastName1} {LastName2}";
    }
}
