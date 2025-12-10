namespace DiscordClone.Models
{
    public class Reaction
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string Emoji { get; set; }
        public int Count { get; set; }

        public virtual ICollection<Message> Message { get; set; }
    }
}
