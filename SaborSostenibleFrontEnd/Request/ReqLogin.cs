using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Request
{
    public class ReqLogin
    {
        [Required(ErrorMessage = "Debe ingresar un email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Debe ingresar una contraseña")]
        public string Password { get; set; }
    }
}
