using DiscordClone.Data;
using DiscordClone.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;
    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

    // ─────────────────────────────
    // Wysyłanie wiadomości
    // ─────────────────────────────
    public async Task SendMessage(int channelId, int userId, string message)
    {
        var user = await _context.UserProfiles.FindAsync(userId);

        var chatMessage = new Message
        {
            ChannelId = channelId,
            UserId = userId,
            Value = message,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(chatMessage);
        await _context.SaveChangesAsync();

        await Clients.All.SendAsync("ReceiveMessage", chatMessage.Id, user.Username, message, chatMessage.CreatedAt);
    }

    // ─────────────────────────────
    // Dodawanie reakcji
    // ─────────────────────────────
    public async Task AddReaction(int messageId, string emoji)
    {
        var reaction = await _context.Reactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.Emoji == emoji);

        if (reaction != null)
            reaction.Count++;
        else
            _context.Reactions.Add(new Reaction { MessageId = messageId, Emoji = emoji, Count = 1 });

        await _context.SaveChangesAsync();

        await Clients.All.SendAsync("ReceiveReaction", messageId, emoji, reaction?.Count ?? 1);
    }

    // ─────────────────────────────
    // Edycja wiadomości
    // ─────────────────────────────
    public async Task EditMessage(int messageId, string newValue)
    {
        var msg = await _context.Messages.FindAsync(messageId);
        if (msg == null) return;

        msg.Value = newValue;
        await _context.SaveChangesAsync();

        await Clients.All.SendAsync("MessageEdited", messageId, newValue);
    }

    // ─────────────────────────────
    // Usuwanie wiadomości
    // ─────────────────────────────
    public async Task DeleteMessage(int messageId)
    {
        var msg = await _context.Messages.FindAsync(messageId);
        if (msg == null) return;

        _context.Messages.Remove(msg);
        await _context.SaveChangesAsync();

        await Clients.All.SendAsync("MessageDeleted", messageId);
    }
}
