using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaborSostenibleFrontEnd.Request
{
    public class ReqSignUp
    {
        [Required(ErrorMessage = "Debe ingresar un email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Debe ingresar una contraseña")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Debe ingresar un nombre")]
        public string FirstName1 { get; set; }

        public string? FirstName2 { get; set; } = "";

        [Required(ErrorMessage = "Debe ingresar un apellido")]
        public string LatestName1 { get; set; }

        public string? LastName2 { get; set; } = "";
        public string? PhoneNumber { get; set; } = "";
        public string? Address { get; set; } = "";
        public decimal? Latitude { get; set; } = null;
        public decimal? Longitude { get; set; } = null;
    }
}
