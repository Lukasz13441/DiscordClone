using System.Threading.Channels;

namespace DiscordClone.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public int UserId { get; set; }
        public int ChannelId { get; set; }

        // Navigation
        public virtual UserProfile User { get; set; }
        public virtual Channel Channel { get; set; }
    }

}
