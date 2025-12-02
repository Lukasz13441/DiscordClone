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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------
            // USER PROFILE
            // -------------------------
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("UserProfile");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.AvatarURL).HasMaxLength(50);
                entity.Property(x => x.BIO).HasMaxLength(190);
                entity.Property(x => x.Username).HasMaxLength(50);
                entity.HasOne(x => x.User)
                  .WithOne(u => u.Profile)
                  .HasForeignKey<UserProfile>(x => x.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------
            // MESSAGE
            // -------------------------
            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Message");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Value).HasMaxLength(2000);

                entity.HasOne(x => x.User)
                      .WithMany(u => u.Messages)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Channel)
                      .WithMany(c => c.Messages)
                      .HasForeignKey(x => x.ChannelId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------
            // CHANNEL
            // -------------------------
            modelBuilder.Entity<Channel>(entity =>
            {
                entity.ToTable("Channel");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name).HasMaxLength(2000);

                entity.HasOne(x => x.Server)
                      .WithMany(c => c.Channel)
                      .HasForeignKey(x => x.ServerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Friendships)
                     .WithMany(c => c.Channel)
                     .HasForeignKey(x => x.FriendShipId)
                     .OnDelete(DeleteBehavior.Cascade);

                entity.Property(x => x.Name).HasMaxLength(50);
            });

            // -------------------------
            // FRIENDSHIP
            // -------------------------
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

            // -------------------------
            // SERVER
            // -------------------------
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

            // -------------------------
            // SERVER MEMBER
            // -------------------------
            modelBuilder.Entity<ServerMember>(entity =>
            {
                entity.ToTable("ServerMember");
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.User)
                      .WithMany(u => u.ServerMembers)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Server)
                      .WithMany(s => s.Members)
                      .HasForeignKey(x => x.ServerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------
            // ROLE
            // -------------------------
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name).HasMaxLength(50);
                entity.Property(x => x.Color).HasMaxLength(50);

                entity.HasOne(x => x.ServerMember)
                      .WithMany(m => m.Roles)
                      .HasForeignKey(x => x.ServerMemberId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Server)
                      .WithMany(s => s.Roles)
                      .HasForeignKey(x => x.ServerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------
            // VOICE CHANNEL
            // -------------------------
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
        }
    }
}
