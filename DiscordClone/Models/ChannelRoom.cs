namespace DiscordClone.Models
{
    public class ChannelRoom
    {
        public int Id { get; set; }
        public int? ChannelId { get; set; }          // For text channels (nullable)
        public int? VoiceChannelId { get; set; }     // For voice channels (nullable)
        public int UserId { get; set; }              // Fixed casing from userId
        public string ConnectionId { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Channel Channel { get; set; }
        public virtual VoiceChannel VoiceChannel { get; set; }
        public virtual UserProfile User { get; set; } = null!;
    }
}