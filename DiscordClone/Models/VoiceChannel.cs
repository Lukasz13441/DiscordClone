namespace DiscordClone.Models
{
    public class VoiceChannel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int VoiceChannelId { get; set; }

        // Navigation
        public virtual Server Server { get; set; }
    }

}
