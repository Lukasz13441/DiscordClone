namespace DiscordClone.Models
{
    public class ChannelRoom
    {
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public int userId { get; set; } = default!;
        public string ConnectionId { get; set; } = default!;

        public virtual Channel Channel { get; set; } 
        public virtual  UserProfile User { get; set; }
        
    }
}
