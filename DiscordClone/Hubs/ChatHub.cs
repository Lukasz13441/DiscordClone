using DiscordClone.Data;
using DiscordClone.Models; // Upewnij się, że tu są Twoje modele (Message, MessageReaction, ChannelRoom)
using DiscordClone.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;


namespace DiscordClone.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;

        }

        // ─────────────────────────────
        // 1. ZARZĄDZANIE POŁĄCZENIEM (OnConnected)
        // ─────────────────────────────
        public override async Task OnConnectedAsync()
        {
            // Pobieramy channelId z adresu URL połączenia (np. /chathub?channelId=5)
            var channelId = Context.GetHttpContext()?.Request.Query["channelId"];
            if (int.TryParse(channelId, out int cid))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Channel_{cid}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            // Sprzątanie: usuwamy rekord z ChannelRooms po rozłączeniu
            var row = await _context.ChannelRooms.FirstOrDefaultAsync(x => x.ConnectionId == connectionId);
            if (row is not null)
            {
                _context.ChannelRooms.Remove(row);
                await _context.SaveChangesAsync();
            }

            await base.OnDisconnectedAsync(exception);
        }

         public async Task LeaveApp(string userId)
        {
            var user = await _context.UserProfiles
            .FirstOrDefaultAsync(us => us.UserId == userId);

            if (user != null)
            {
                user.activityStatus = ActivityStatus.Offline;
                _context.Update(user);
                _context.SaveChanges();
            }
        }

        // ─────────────────────────────
        // 2. LOGIKA UŻYTKOWNIKA I OBECNOŚCI
        // ─────────────────────────────
        public async Task JoinChannel(int userId, int ChannelId)
        {
            var connectionId = Context.ConnectionId;

            // Sprawdzamy, czy połączenie już istnieje
            var existing = await _context.ChannelRooms
                .Include(cr => cr.User) // Zakładam, że w modelu ChannelRoom jest nawigacja do User
                .FirstOrDefaultAsync(x => x.ConnectionId == connectionId);

            if (existing is null)
            {
                // Tworzymy nowy wpis
                _context.ChannelRooms.Add(new ChannelRoom
                {
                    userId = userId, // Poprawiona wielkość liter (zależnie od Twojego modelu)
                    ConnectionId = connectionId,
                    ChannelId = ChannelId
                    // LastSeenUtc = DateTime.UtcNow // Opcjonalnie, jeśli masz to pole
                });
            }
            else
            {
                // Aktualizujemy istniejący
                existing.userId = userId;
            }

            await _context.SaveChangesAsync();

            // Pobieramy nazwę użytkownika do powiadomienia (opcjonalnie)
            var user = await _context.UserProfiles.FindAsync(userId);
            string username = user?.Username ?? "Unknown";

            await Clients.Caller.SendAsync("ReceiveNotification",
                $"✅ Połączono z serwerem jako: {username}");
        }

        

        public async Task Ping(string userId)
        {
            var user = await _context.UserProfiles
            .FirstOrDefaultAsync(us => us.UserId == userId);

            

            if (user == null) {
                user.activityStatus = ActivityStatus.Offline;
                _context.Update(user);
                _context.SaveChanges();
                return;
            }

               var friends = await _context.Friendships
                .Where(f => (f.UserId == user.Id || f.FriendId == user.Id) &&
                           (f.Status == Status.Accepted || f.Status == Status.Chating))
                .Select(f => f.UserId == user.Id ? f.FriendId : f.UserId)
                .Join(_context.UserProfiles,
                      friendId => friendId,
                      u => u.Id,
                      (friendId, profile) => profile)
                .ToListAsync(); 
  
            user.activityStatus = ActivityStatus.Online;
                _context.Update(user);
                _context.SaveChanges();

            await Clients.Caller.SendAsync("ActivityStatus",
                $"uzytkownik {user.Username} jest onlie", user.activityStatus);

            foreach (var friend in friends)
            {
                await Clients.Caller.SendAsync(
                    "FriendActivityStatus",
                    $"uzytkownik {friend.Username} jest onlie",
                    friend.activityStatus,
                    friend.Id
                );
            }
        }
    // ─────────────────────────────
    // 3. WYSYŁANIE WIADOMOŚCI
    // ─────────────────────────────
    public async Task SendMessage(int channelId, int userId, string message)
        {
            var user = await _context.UserProfiles.FindAsync(userId);
            if (user == null) return;

            var chatMessage = new Message
            {
                ChannelId = channelId,
                UserId = userId,
                Value = message,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(chatMessage);
            await _context.SaveChangesAsync();

            var reactions = new List<object>();

            // Wysyłamy do grupy (kanału), a nie do wszystkich (Clients.All)
            // Dzięki temu wiadomości z kanału 1 nie trafią na kanał 2
            await Clients.Group($"Channel_{channelId}").SendAsync("ReceiveMessage",
                chatMessage.Id,
                user.Username,
                message,
                chatMessage.CreatedAt,
                reactions);
        }

        // ─────────────────────────────
        // 4. REAKCJE
        // ─────────────────────────────
        public async Task ToggleReaction(int messageId, int reactionId, int userId)
        {
            try
            {
                string reactionKey = reactionId.ToString();

                var message = await _context.Messages.FindAsync(messageId);
                if (message == null) return;

                var existingReaction = await _context.MessageReactions
                    .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == reactionKey);

                if (existingReaction != null)
                {
                    _context.MessageReactions.Remove(existingReaction);
                }
                else
                {
                    _context.MessageReactions.Add(new MessageReaction
                    {
                        MessageId = messageId,
                        UserId = userId,
                        Emoji = reactionKey,
                        Count = 1
                    });
                }

                await _context.SaveChangesAsync();

                int totalCount = await _context.MessageReactions
                    .CountAsync(r => r.MessageId == messageId && r.Emoji == reactionKey);

                // Wysyłamy aktualizację do grupy kanału tej wiadomości
                await Clients.Group($"Channel_{message.ChannelId}").SendAsync("UpdateReaction", messageId, reactionKey, totalCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ToggleReaction: {ex.Message}");
                await Clients.Caller.SendAsync("Error", "Server error processing reaction.");
            }
        }

        // ─────────────────────────────
        // 5. EDYCJA I USUWANIE
        // ─────────────────────────────
        public async Task EditMessage(int messageId, string newValue)
        {
            var msg = await _context.Messages.FindAsync(messageId);
            if (msg == null) return;

            msg.Value = newValue;
            await _context.SaveChangesAsync();

            await Clients.Group($"Channel_{msg.ChannelId}").SendAsync("MessageEdited", messageId, newValue);
        }

        public async Task DeleteMessage(int messageId)
        {
            var msg = await _context.Messages.FindAsync(messageId);
            if (msg == null) return;

            int channelId = msg.ChannelId; // Zapamiętujemy ID przed usunięciem

            _context.Messages.Remove(msg);
            await _context.SaveChangesAsync();

            await Clients.Group($"Channel_{channelId}").SendAsync("MessageDeleted", messageId);
        }
    }
}