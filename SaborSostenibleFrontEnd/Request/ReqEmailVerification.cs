using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Request
{
    public class ReqEmailVerification
    {
        [Required(ErrorMessage = "Debe ingresar un email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Debe ingresar un código")]
        public string VerificationCode { get; set; }
        
    }
}
