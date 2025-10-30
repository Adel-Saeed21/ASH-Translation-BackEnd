using System.ComponentModel.DataAnnotations;

namespace ASH_Translation.Models.DTO
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Refresh Token is Required")]
        public string RefreshToken { get; set; }
    }
}

