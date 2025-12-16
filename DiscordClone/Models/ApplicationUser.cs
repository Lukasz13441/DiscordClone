using Microsoft.AspNetCore.Identity;
namespace DiscordClone.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual UserProfile Profile { get; set; }
    }

}
