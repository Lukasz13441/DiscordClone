using System.Threading.Channels;

namespace DiscordClone.Models
{
    public class Friendship
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int FriendId { get; set; }

        public string Status { get; set; }

        // Navigation
        public virtual UserProfile User { get; set; }

        public virtual ICollection<Channel> Channel { get; set; }
    }

}
