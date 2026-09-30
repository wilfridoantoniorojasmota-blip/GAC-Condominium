using System.ComponentModel.DataAnnotations;

namespace PROTOTIPO.Models.ViewModel
{
    public class LoginVM
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string Rol { get; set; }
    }
}
