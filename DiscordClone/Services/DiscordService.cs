using DiscordClone.Data;
using DiscordClone.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DiscordClone.Services
{
    public class DiscordService
    {
        private readonly ApplicationDbContext _context;

        public DiscordService(ApplicationDbContext context)
        {
            _context = context;
        }

        public string[] SplitName(string text)
        {
            string[] parts = text.Split('#');
            return new string[] { parts[0], parts[1] };
        }

        // Pobieranie serwerów użytkownika
        public List<Server> GetUserServers(string userId)
        {
            var userProfile = _context.UserProfiles.FirstOrDefault(x => x.UserId == userId);
            if (userProfile == null) return new List<Server>();

            return _context.Servers
                .Where(s => s.OwnerId == userProfile.Id ||
                            _context.ServerMembers.Any(sm => sm.ServerId == s.Id && sm.UserId == userProfile.Id))
                .ToList();
        }

        // Pobieranie znajomych
        public List<UserProfile> GetUserFriends(string userId)
        {
            var userProfile = _context.UserProfiles.FirstOrDefault(x => x.UserId == userId);
            if (userProfile == null) return new List<UserProfile>();

            return _context.Friendships
                .Where(f => f.UserId == userProfile.Id && f.Status == Status.Accepted)
                .Join(_context.UserProfiles,
                      f => f.FriendId,
                      u => u.Id,
                      (f, u) => u)
                .ToList();
        }

        // Lista oczekujących zaproszeń
        public List<UserProfile> GetPendingFriendRequests(string userId)
        {
            var userProfile = _context.UserProfiles.FirstOrDefault(x => x.UserId == userId);
            if (userProfile == null) return new List<UserProfile>();

            return _context.Friendships
                .Where(f => f.FriendId == userProfile.Id && f.Status == Status.Pending)
                .Join(_context.UserProfiles,
                      f => f.UserId,
                      u => u.Id,
                      (f, u) => u)
                .ToList();
        }

        public void CreateServer(string userId, Server model)
        {
            var userProfile = _context.UserProfiles.FirstOrDefault(x => x.UserId == userId);
            if (userProfile == null) return;

            _context.Servers.Add(new Server
            {
                Name = model.Name,
                OwnerId = userProfile.Id,
                CreatedAt = DateTime.Now,
                IconURL = "/images/default_avatar.png"
            });
            
            _context.SaveChanges();
            CreateDefaultChannel(userProfile.Id);
        }

        public void CreateDefaultChannel(int UserId)
        {
            var Server = _context.Servers
                .Where(x => x.OwnerId == UserId)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefault();

            Channel defaultChannel = new Channel
            {
                ServerId = Server.Id,
                Name = "general"
            };

            _context.Channels.Add(defaultChannel);
            _context.SaveChanges();
        }

       
        public void AddFriend(string userId, string usernameInput)
        {
            var userProfile = _context.UserProfiles.FirstOrDefault(x => x.UserId == userId);
            if (userProfile == null || string.IsNullOrEmpty(usernameInput)) return;

            try
            {
            var nameTag = SplitName(usernameInput);
            var friend = _context.UserProfiles
                .FirstOrDefault(f => f.Username == nameTag[0] && f.Tag == int.Parse(nameTag[1]));

            _context.Friendships.Add(new Friendship
            {
                UserId = userProfile.Id,
                FriendId = friend.Id,
                Status = Status.Pending
            });

            _context.SaveChanges();


            }
            catch
            {
                return;
            }
    
        }

        public void AcceptFriendship(string userId, int friendId)
        {
            var userProfile = _context.UserProfiles.FirstOrDefault(x => x.UserId == userId);
            if (userProfile == null) return;

            var friendship = _context.Friendships.FirstOrDefault(f =>
                f.FriendId == userProfile.Id && f.UserId == friendId);

            if (friendship != null)
            {
                friendship.Status = Status.Accepted;

                _context.Friendships.Add(new Friendship
                {
                    UserId = userProfile.Id,
                    FriendId = friendId,
                    Status = Status.Accepted
                });

                _context.SaveChanges();
            }
        }

        public List<Channel> GetChanels(int ServerId)
        {
            var channels = _context.Channels
                .Where(f => f.ServerId == ServerId)
                .ToList();
            if (channels == null) return null;

            return channels;
        }

        public Server GetServerById(int ServerId)
        {
            var Server = _context.Servers
                .FirstOrDefault(f => f.Id == ServerId);

            if (Server == null) return null;

            return Server;
        }

        public void CreateChannel(int serverId, Channel model)
        {
            var server = _context.Servers.FirstOrDefault(s => s.Id == serverId);
            if (server == null) return;
            _context.Channels.Add(new Channel
            {
                Name = model.Name,
                ServerId = server.Id
            });
            _context.SaveChanges();
        }
    }
}
