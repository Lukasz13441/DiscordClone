// MessageReaction.cs  (nowy plik)
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordClone.Models
{
    [Table("MessageReactions")]  // <<< TO JEST NAJWAŻNIEJSZE!
    public class MessageReaction
    {
        public int Id { get; set; }
        public string Emoji { get; set; } = string.Empty;
        public int Count { get; set; }
        public int UserId { get; set; }
        public virtual UserProfile User { get; set; } = null!;

        public int MessageId { get; set; }
        public virtual Message Message { get; set; } = null!;
    }
}