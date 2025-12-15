using DiscordClone.Data;
using DiscordClone.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DiscordClone.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        // Wysyłanie wiadomości
        public async Task SendMessage(int channelId, int userId, string message)
        {
            var user = await _context.UserProfiles.FindAsync(userId);
            if (user == null) return;

            var newMessage = new Message
            {
                Value = message,
                ChannelId = channelId,
                UserId = userId,  // Changed from UserProfileId to UserId
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage",
                newMessage.Id,
                user.Username,
                message,
                newMessage.CreatedAt);
        }

        // Edycja wiadomości
        public async Task EditMessage(int messageId, string newText)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null) return;

            message.Value = newText;
            await _context.SaveChangesAsync();

            await Clients.All.SendAsync("MessageEdited", messageId, newText);
        }

        // Usuwanie wiadomości
        public async Task DeleteMessage(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null) return;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            await Clients.All.SendAsync("MessageDeleted", messageId);
        }

        // Toggle reakcji - teraz używa klucza string zamiast emoji
        public async Task ToggleReaction(int messageId, string reactionKey, string userId)
        {
            var userIdInt = int.Parse(userId);

            // Sprawdź, czy użytkownik już zareagował tym kluczem
            var existingReaction = await _context.MessageReactions
                .FirstOrDefaultAsync(r =>
                    r.MessageId == messageId &&
                    r.UserId == userIdInt &&
                    r.Emoji == reactionKey);

            if (existingReaction != null)
            {
                // Użytkownik już dodał tę reakcję → usuń
                _context.MessageReactions.Remove(existingReaction);
            }
            else
            {
                // Dodaj nową reakcję
                var newReaction = new MessageReaction
                {
                    MessageId = messageId,
                    UserId = userIdInt,
                    Emoji = reactionKey,  // Teraz zapisujemy "1", "2", "3" etc. zamiast emoji
                    Count = 1
                };
                _context.MessageReactions.Add(newReaction);
            }

            await _context.SaveChangesAsync();

            // Policz wszystkie reakcje tego typu dla tej wiadomości
            var totalCount = await _context.MessageReactions
                .CountAsync(r => r.MessageId == messageId && r.Emoji == reactionKey);

            // Wyślij update do wszystkich klientów
            await Clients.All.SendAsync("UpdateReaction", messageId, reactionKey, totalCount);
        }
    }
}