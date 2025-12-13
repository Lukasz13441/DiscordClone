using Microsoft.AspNetCore.Identity;
using System.Collections;

namespace DiscordClone.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string AvatarURL { get; set; }
        public string Username { get; set; }
        public string BIO { get; set; }
        public int Tag { get; set; }

        // Navigation

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Server> Server { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<Friendship> Friendships { get; set; }
        public virtual ICollection<ServerMember> ServerMembers { get; set; }

        public virtual List<MessageReaction> Reactions { get; set; } = new();
    }

}
