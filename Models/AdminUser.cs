using Microsoft.AspNetCore.Identity;

namespace ASH_Translation.Models
{
    public class AdminUser:IdentityUser
    {
        public string FullName { get; set; }
    }
}
