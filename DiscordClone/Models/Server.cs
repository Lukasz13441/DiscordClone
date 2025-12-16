using System.Data;
using System.Threading.Channels;

namespace DiscordClone.Models
{
    public class Server
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public int OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IconURL { get; set; }


        // Navigation
        public virtual ICollection<ServerMember> Members { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public virtual ICollection<Channel> Channel { get; set; }
        public virtual ICollection<VoiceChannel> VoiceChannel { get; set; }

        public virtual UserProfile User { get; set; }
    }

}
