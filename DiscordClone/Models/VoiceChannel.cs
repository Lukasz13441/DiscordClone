using DiscordClone.Models;
    public class VoiceChannel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ServerId { get; set; }

    // Navigation
    public virtual Server Server { get; set; }
}
