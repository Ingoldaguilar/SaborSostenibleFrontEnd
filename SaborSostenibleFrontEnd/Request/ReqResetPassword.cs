using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Request
{
    public class ReqResetPassword
    {
        public string Email { get; set; }
        public string PasswordHash1 { get; set; }
        public string PasswordHash2 { get; set; }
        public string VerificationCode { get; set; }
    }
}
