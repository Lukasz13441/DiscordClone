using Microsoft.AspNetCore.Identity;
using System.Data;

namespace DiscordClone.Models
{
    public class ServerMember 
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int ServerId { get; set; }
        public Range Range { get; set; }
        public bool IsBanned { get; set; } = false;

        // Navigation
        public virtual UserProfile User { get; set; }
        public virtual Server Server { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
    }
    public enum Range
    {
        Owner, 
        Admin, 
        Moderator, 
        Member
    }

}
