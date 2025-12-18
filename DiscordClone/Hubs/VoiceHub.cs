using DiscordClone.Data;
using DiscordClone.Models;
using DiscordClone.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DiscordClone.Hubs
{
    public class VoiceHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public VoiceHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task JoinVoiceChannel(int voiceChannelId, int userId)
        {
            try
            {
                // Check if voice channel exists
                var voiceChannel = await _context.VoiceChannels
                    .Include(vc => vc.Server)
                    .FirstOrDefaultAsync(vc => vc.Id == voiceChannelId);

                if (voiceChannel == null)
                {
                    await Clients.Caller.SendAsync("Error", "Voice channel not found");
                    return;
                }

                // Remove user from any existing voice channels
                var existingRooms = await _context.ChannelRooms
                    .Where(cr => cr.UserId == userId && cr.VoiceChannelId != null)
                    .ToListAsync();

                if (existingRooms.Any())
                {
                    _context.ChannelRooms.RemoveRange(existingRooms);
                }

                // Create new room entry
                var channelRoom = new ChannelRoom
                {
                    VoiceChannelId = voiceChannelId,  // Use VoiceChannelId
                    UserId = userId,
                    ConnectionId = Context.ConnectionId,
                    JoinedAt = DateTime.UtcNow
                };

                _context.ChannelRooms.Add(channelRoom);
                await _context.SaveChangesAsync();

                // Get updated list of users in this voice channel
                var usersInChannel = await _context.ChannelRooms
                    .Where(cr => cr.VoiceChannelId == voiceChannelId)
                    .Include(cr => cr.User)
                    .Select(cr => new
                    {
                        cr.User.Id,
                        cr.User.Username,
                        cr.User.AvatarURL,
                        cr.User.activityStatus
                    })
                    .ToListAsync();

                // Notify all clients in the server about the update
                await Clients.All.SendAsync("UserJoinedVoice", voiceChannelId, usersInChannel);

                Console.WriteLine($"✅ User {userId} joined voice channel {voiceChannelId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error joining voice channel: {ex.Message}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task LeaveVoiceChannel(int userId)
        {
            try
            {
                var channelRoom = await _context.ChannelRooms
                    .FirstOrDefaultAsync(cr => cr.UserId == userId
                        && cr.ConnectionId == Context.ConnectionId
                        && cr.VoiceChannelId != null);

                if (channelRoom != null)
                {
                    var voiceChannelId = channelRoom.VoiceChannelId.Value;
                    _context.ChannelRooms.Remove(channelRoom);
                    await _context.SaveChangesAsync();

                    // Get updated list of users
                    var usersInChannel = await _context.ChannelRooms
                        .Where(cr => cr.VoiceChannelId == voiceChannelId)
                        .Include(cr => cr.User)
                        .Select(cr => new
                        {
                            cr.User.Id,
                            cr.User.Username,
                            cr.User.AvatarURL,
                            cr.User.activityStatus
                        })
                        .ToListAsync();

                    await Clients.All.SendAsync("UserLeftVoice", voiceChannelId, usersInChannel);

                    Console.WriteLine($"✅ User {userId} left voice channel {voiceChannelId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error leaving voice channel: {ex.Message}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Remove user from voice channel on disconnect
            var channelRoom = await _context.ChannelRooms
                .FirstOrDefaultAsync(cr => cr.ConnectionId == Context.ConnectionId
                    && cr.VoiceChannelId != null);

            if (channelRoom != null)
            {
                var voiceChannelId = channelRoom.VoiceChannelId.Value;
                _context.ChannelRooms.Remove(channelRoom);
                await _context.SaveChangesAsync();

                var usersInChannel = await _context.ChannelRooms
                    .Where(cr => cr.VoiceChannelId == voiceChannelId)
                    .Include(cr => cr.User)
                    .Select(cr => new
                    {
                        cr.User.Id,
                        cr.User.Username,
                        cr.User.AvatarURL,
                        cr.User.activityStatus
                    })
                    .ToListAsync();

                await Clients.All.SendAsync("UserLeftVoice", voiceChannelId, usersInChannel);

                Console.WriteLine($"✅ User disconnected from voice channel {voiceChannelId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}