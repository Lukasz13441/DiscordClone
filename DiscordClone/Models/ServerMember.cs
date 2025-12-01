using System.Data;

namespace DiscordClone.Models
{
    public class ServerMember
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int ServerId { get; set; }

        // Navigation
        public virtual UserProfile User { get; set; }
        public virtual Server Server { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
    }

}
