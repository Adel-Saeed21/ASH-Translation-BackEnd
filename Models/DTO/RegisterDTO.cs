using System.ComponentModel.DataAnnotations;

namespace ASH_Translation.Models.DTO
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Your Name is required")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
