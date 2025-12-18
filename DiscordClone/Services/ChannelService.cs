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
        private readonly IWebHostEnvironment _env;

        public ChannelService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public void CreateDefaultChannel(int ServerId)
        {

            Channel defaultChannel = new Channel
            {
                ServerId = ServerId,
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
            if (channels == null)
            {
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



        public List<object> GetChannelMessages(int channelId)
        {
            return _context.Messages
                .Where(m => m.ChannelId == channelId)
                .Include(m => m.User)
                .Include(m => m.Reactions).ThenInclude(r => r.User)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    m.Id,
                    m.Value,
                    m.CreatedAt,
                    User = new
                    {
                        m.User.Id,
                        m.User.Username,
                        m.User.AvatarURL
                    },
                    // Grupujemy reakcje po emoji – dokładnie jak na Discordzie
                    Reactions = m.Reactions
                        .GroupBy(r => r.Emoji)
                        .Select(g => new
                        {
                            Emoji = g.Key,
                            Count = g.Count(),
                            Users = g.Select(r => new
                            {
                                r.User.Id,
                                r.User.Username
                            }).ToList()
                        })
                        .Where(g => g.Count > 0)
                        .ToList()
                })
                .ToList<object>(); // <-- tutaj rzutujemy na object, żeby ViewBag przyjął
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

        public async Task UpdateUserProfileAsync(int userId, UserProfileFile model)
        {
            var user = await _context.UserProfiles.FindAsync(userId);

            if (user == null)
                return;

            // Aktualizacja pól tekstowych
            if (model.Username != null) user.Username = model.Username;

            if (model.BIO != null) user.BIO = model.BIO;

            // Obsługa avatara
            if (model.AvatarURL != null)
            {
                user.AvatarURL = await SaveAvatarAsync(model.AvatarURL, user.AvatarURL);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<string> SaveAvatarAsync(IFormFile file, string? oldAvatarPath)
        {
            string avatarsFolder = Path.Combine(_env.WebRootPath, "avatars");
            Directory.CreateDirectory(avatarsFolder);

            // Usuń stary avatar (jeśli istnieje)
            if (!string.IsNullOrEmpty(oldAvatarPath))
            {
                var oldFilePath = Path.Combine(_env.WebRootPath, oldAvatarPath.TrimStart('/'));
                if (File.Exists(oldFilePath))
                    File.Delete(oldFilePath);
            }

            // Generuj unikalną nazwę pliku
            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid()}{extension}";
            string fullPath = Path.Combine(avatarsFolder, fileName);

            // Zapisz plik
            await using var stream = File.Create(fullPath);
            await file.CopyToAsync(stream);

            return $"/avatars/{fileName}";
        }

        //voice channel part

        public void CreateVoiceChannel(int serverId, string name)
        {
            var server = _context.Servers.FirstOrDefault(s => s.Id == serverId);
            if (server == null) return;

            _context.VoiceChannels.Add(new VoiceChannel
            {
                Name = name,
                ServerId = serverId
            });
            _context.SaveChanges();
        }



        // Add method to get voice channels
        public List<VoiceChannel> GetVoiceChannels(int serverId)
        {
            return _context.VoiceChannels
                .Where(vc => vc.ServerId == serverId)
                .ToList();
        }
    }
}
