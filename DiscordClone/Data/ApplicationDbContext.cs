using DiscordClone.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiscordClone.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<ServerMember> ServerMembers { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<VoiceChannel> VoiceChannels { get; set; }

        // FIXED: Changed from "Reactions" to "MessageReactions" and removed the duplicate object line
        public DbSet<MessageReaction> MessageReactions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. NAJPIERW ZABIJAMY WSZYSTKIE CASCADE – TO MUSI BYĆ PIERWSZA LINIA
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // 2. TERAZ DOPIERO KONFIGURACJA – WSZYSTKO JEST JUŻ Restrict, więc zero błędów
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("UserProfile");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Username).HasMaxLength(50).IsRequired();
                entity.Property(x => x.AvatarURL).HasMaxLength(500);
                entity.Property(x => x.BIO).HasMaxLength(190);

                entity.HasOne(x => x.User)
                      .WithOne(u => u.Profile)
                      .HasForeignKey<UserProfile>(x => x.UserId);
                // tu zostaje Cascade – to jedyna bezpieczna relacja 1:1
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Message");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Value).HasMaxLength(4000).IsRequired();
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(x => x.User).WithMany(u => u.Messages).HasForeignKey(x => x.UserId);
                entity.HasOne(x => x.Channel).WithMany(c => c.Messages).HasForeignKey(x => x.ChannelId);

                entity.HasMany(x => x.Reactions).WithOne(r => r.Message).HasForeignKey(r => r.MessageId)
                      .OnDelete(DeleteBehavior.Cascade); // tylko reakcje mogą się kasować kaskadowo
            });

            modelBuilder.Entity<MessageReaction>(entity =>
            {
                entity.ToTable("MessageReactions");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Emoji).HasMaxLength(100).IsRequired();

                entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
                entity.HasOne(x => x.Message).WithMany(m => m.Reactions).HasForeignKey(x => x.MessageId);

                entity.HasIndex(x => new { x.MessageId, x.UserId, x.Emoji }).IsUnique();
            });

            modelBuilder.Entity<Channel>(entity =>
            {
                entity.ToTable("Channel");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();

                entity.HasOne(x => x.Server).WithMany(s => s.Channel).HasForeignKey(x => x.ServerId);
                entity.HasOne(x => x.Friendships).WithMany(f => f.Channel).HasForeignKey(x => x.FriendShipId);
            });

            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.ToTable("Friendship");
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.User)
                      .WithMany(u => u.Friendships)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.User)
                      .WithMany(u => u.Friendships)
                      .HasForeignKey(x => x.FriendId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Server>(entity =>
            {
                entity.ToTable("Server");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name).HasMaxLength(50);

                entity.HasOne(x => x.User)
                      .WithMany(u => u.Server)
                      .HasForeignKey(x => x.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ServerMember>(entity =>
            {
                entity.ToTable("ServerMember");
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.User).WithMany(u => u.ServerMembers).HasForeignKey(x => x.UserId);
                entity.HasOne(x => x.Server).WithMany(s => s.Members).HasForeignKey(x => x.ServerId);
                entity.HasIndex(x => new { x.ServerId, x.UserId }).IsUnique();
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(50);
                entity.Property(x => x.Color).HasMaxLength(7);

                entity.HasOne(x => x.ServerMember).WithMany(m => m.Roles).HasForeignKey(x => x.ServerMemberId);
                entity.HasOne(x => x.Server).WithMany(s => s.Roles).HasForeignKey(x => x.ServerId);
            });

            modelBuilder.Entity<VoiceChannel>(entity =>
            {
                entity.ToTable("VoiceChannel");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name).HasMaxLength(50);

                entity.HasOne(x => x.Server)
                      .WithMany(s => s.VoiceChannel)
                      .HasForeignKey(x => x.VoiceChannelId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}