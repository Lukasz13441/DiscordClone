using System.Collections;

namespace DiscordClone.Models
{
    public class Channel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? ServerId { get; set; }

        public int? FriendShipId { get; set; }

        // Navigation
        public virtual ICollection<Message> Messages { get; set; }
        
        public virtual ICollection<ChannelRoom> ChannelRoom { get; set; }
        public virtual Friendship Friendships { get; set; }

        public virtual Server Server { get; set; }
        
    }

}
