using DiscordClone.Data;
using DiscordClone.Models;
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;

    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

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

        // Wyślij do wszystkich
        await Clients.All.SendAsync("ReceiveMessage", user.Username, message);
    }
}
