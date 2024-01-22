using Microsoft.AspNetCore.Identity;

namespace KeepPasswords.Models.Account
{
    public class User : IdentityUser
    {
        public string? Location { get; set; }
    }
}
