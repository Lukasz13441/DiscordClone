namespace DiscordClone.Models
{
    public class Role
    {
        public int Id { get; set; }

        public int ServerMemberId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public int ServerId { get; set; }

        // Navigation
        public virtual ServerMember ServerMember { get; set; }
        public virtual Server Server { get; set; }
    }

}
