using DiscordClone.Data;
using DiscordClone.Models;
using Microsoft.EntityFrameworkCore;


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
            var Server = _context.Servers
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault();

            _ChannelService.CreateDefaultChannel(Server.Id);
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

        public async Task UpdateServerAsync(ServerViewModel model)
        {
            // Użycie FirstOrDefaultAsync (wymaga: using Microsoft.EntityFrameworkCore;)
            var server = await _context.Servers
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (server == null) return;

            if (model.Name != null)
            {
                server.Name = model.Name;
            }

            // Sprawdzenie czy przesłano plik (zakładam, że IconURL to IFormFile)
            if (model.IconURL != null && model.IconURL.Length > 0)
            {
                // Tutaj następuje wywołanie Twojego serwisu zapisującego plik
                server.IconURL = await _ChannelService.SaveAvatarAsync(model.IconURL, server.IconURL);
            }

            // Zmiana SaveChanges na wersję asynchroniczną
            await _context.SaveChangesAsync();
        }

    }
}
