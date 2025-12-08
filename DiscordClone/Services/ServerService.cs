using DiscordClone.Data;
using DiscordClone.Models;


namespace DiscordClone.Services
{
    public class ServerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ChannelService _ChannelService;
        private readonly FriendsService _FriendsService;

        public ServerService(ApplicationDbContext context, FriendsService FriendsService, ChannelService channelService, FriendsService friendsService)
        {
            _context = context;
            _ChannelService = channelService;
            _FriendsService = friendsService;
        }

        public List<Server> GetUserServers(string userId)
        {
            var UserId = _FriendsService.GetUserIntId(userId);
            if (UserId == null) return null;

            return _context.Servers
                .Where(s => s.OwnerId == UserId ||
                            _context.ServerMembers.Any(sm => sm.ServerId == s.Id && sm.UserId == UserId))
                .ToList();
        }

        public void CreateServer(string userId, Server model)
        {
            var UserId = _FriendsService.GetUserIntId(userId);

            _context.Servers.Add(new Server
            {
                Name = model.Name,
                OwnerId = UserId,
                CreatedAt = DateTime.Now,
                IconURL = "/images/default_avatar.png"
            });
            
            _context.SaveChanges();
            _ChannelService.CreateDefaultChannel(UserId);
        }

        public Server GetServerById(int ServerId)
        {
            var Server = _context.Servers
                .FirstOrDefault(f => f.Id == ServerId);

            if (Server == null) return null;

            return Server;
        }

        public void JoinServer(int UserId, int ServerId)
        {
            _context.ServerMembers.Add(new ServerMember
            {
                UserId = UserId,
                ServerId = ServerId
            });
            _context.SaveChanges();
        }
    }
}
