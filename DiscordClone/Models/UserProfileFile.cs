using Microsoft.AspNetCore.Identity;
using System.Collections;

namespace DiscordClone.Models
{
    public class UserProfileFile
    {
        public IFormFile AvatarURL { get; set; }
        public string Username { get; set; }
        public string BIO { get; set; }

    }

}
