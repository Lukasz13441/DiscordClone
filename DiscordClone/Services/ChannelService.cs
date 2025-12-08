using DiscordClone.Data;
using DiscordClone.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using Channel = DiscordClone.Models.Channel;

namespace DiscordClone.Services
{
    public class ChannelService
    {
        private readonly ApplicationDbContext _context;

        public ChannelService(ApplicationDbContext context)
        {
            _context = context;
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

        public List<Channel> GetChanels(int ServerId)
        {
            var channels = _context.Channels
                .Where(f => f.ServerId == ServerId)
                .ToList();
            if (channels == null) {
                channels = _context.Channels
                    .Where(f => f.FriendShipId == ServerId)
                    .ToList();
            }
            if (channels == null)
            {
                return null;
            }
            return channels;
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

        //sending message
        public void SendMessage(int channelId, int userId, string messageValue)
        {
            var channel = _context.Channels.FirstOrDefault(c => c.Id == channelId);
            var user = _context.UserProfiles.FirstOrDefault(u => u.Id == userId);
            if (channel == null || user == null || string.IsNullOrEmpty(messageValue)) return;
            _context.Messages.Add(new Message
            {
                ChannelId = channelId,
                UserId = userId,
                Value = messageValue,
                CreatedAt = DateTime.Now
            });
            _context.SaveChanges();
        }

        public List<Message> GetMessages(int channelId)
        {
            var messages = _context.Messages
                .Where(m => m.ChannelId == channelId)
                .Include(m => m.User)
                .OrderBy(m => m.CreatedAt)
                .ToList();
            if (messages == null) return new List<Message>();
            return messages;
        }

        public int GetFirstChannel(int ServerId)
        {
            var Channel = _context.Channels
                .Where(f => f.ServerId == ServerId)
                .OrderBy(f => f.Id)
                .FirstOrDefault();
            if (Channel == null) return 0;
            return Channel.Id;
        }



        public List<Message> GetChannelMessages(int channelId)
        {
            var messages = _context.Messages
                .Where(m => m.ChannelId == channelId)
                .Include(m => m.User)
                .OrderBy(m => m.CreatedAt)
                .ToList();
            if (messages == null) return new List<Message>();
            return messages;
        }

        public Server GetServerFromChannelId(int id)
        {
            var serverId = _context.Channels
                                   .Where(c => c.Id == id)
                                   .Select(c => c.ServerId)
                                   .FirstOrDefault();

            return _context.Servers.Find(serverId);
        }

        public void CreateFriendshipChannel(int FriendshipId)
        {
            if (_context.Channels.Any(c => c.FriendShipId == FriendshipId))
            {
                return; 
            }
            _context.Channels.Add(new Channel
            {
                Name = "Chat",
                FriendShipId = FriendshipId
            });
            _context.SaveChanges();
        }

        public int GetFriendshipChannelId(int FriendshipId)
        {
            var channel = _context.Channels
                .FirstOrDefault(c => c.FriendShipId == FriendshipId);
            if (channel == null) return 0;
            return channel.Id;
        }
    }
}
