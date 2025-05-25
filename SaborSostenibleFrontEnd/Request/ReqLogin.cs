using System.ComponentModel.DataAnnotations;

namespace SaborSostenibleFrontEnd.Request
{
    public class ReqLogin
    {
        [Required(ErrorMessage = "Debe ingresar un email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar una contraseña")]
        public string PasswordHash { get; set; } = string.Empty;

        // Property para facilitar el uso en el código
        public string Password
        {
            get => PasswordHash;
            set => PasswordHash = value;
        }
    }
}