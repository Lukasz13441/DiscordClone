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
    public async Task AddReaction(int messageId, string emoji, int userId)
    {
        var existing = await _context.Reactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji);

        if (existing != null)
        {
            // Usuń reakcję (toggle off)
            _context.Reactions.Remove(existing);
        }
        else
        {
            // Dodaj reakcję (toggle on)
            _context.Reactions.Add(new MessageReaction
            {
                MessageId = messageId,
                UserId = userId,
                Emoji = emoji
            });
        }

        await _context.SaveChangesAsync();

        // Pobierz aktualną listę użytkowników którzy zareagowali tym emoji
        var currentUsers = await _context.Reactions
            .Where(r => r.MessageId == messageId && r.Emoji == emoji)
            .Select(r => new { r.UserId, r.User.Username })
            .ToListAsync();

        var count = currentUsers.Count;
        var usernames = currentUsers.Select(u => u.Username).ToList();

        // Wyślij aktualizację do wszystkich w kanale
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null)
        {
            await Clients.Group($"Channel_{message.ChannelId}").SendAsync("ReactionUpdated",
                messageId,
                emoji,
                count,
                userId,
                usernames // pełna lista nazw!
            );
        }
    }

    public override async Task OnConnectedAsync()
    {
        var channelId = Context.GetHttpContext()?.Request.Query["channelId"];
        if (int.TryParse(channelId, out int cid))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Channel_{cid}");
        }
        await base.OnConnectedAsync();
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
    // ─────────────────────────────
    // reakcjie
    // ─────────────────────────────
    public async Task ToggleReaction(int messageId, string emoji, string userIdString)
    {
        // Konwersja ID usera (bo z JS przychodzi jako string)
        if (!int.TryParse(userIdString, out int userId)) return;

        // 1. Sprawdź, czy ten użytkownik już zareagował tą konkretną emotką
        var existingReaction = _context.Reactions
            .FirstOrDefault(r =>
                r.MessageId == messageId &&
                r.UserId == userId &&
                r.Emoji == emoji);

        // 2. Logika PRZEŁĄCZNIKA (Toggle)
        if (existingReaction != null)
        {
            // Jeśli reakcja istnieje -> USUŃ JĄ (Użytkownik cofa lajka)
            _context.Reactions.Remove(existingReaction);
        }
        else
        {
            // Jeśli reakcji nie ma -> DODAJ JĄ
            var newReaction = new MessageReaction
            {
                MessageId = messageId,
                UserId = userId, // Tutaj wykorzystujemy Twoje pole UserId
                Emoji = emoji,
                Count = 1 // Ustawiamy 1, chociaż w tym modelu pole Count jest technicznie zbędne, bo liczymy wiersze
            };
            _context.Reactions.Add(newReaction);
        }

        // 3. Zapisz zmiany w bazie
        await _context.SaveChangesAsync();

        // 4. Policz, ile w sumie osób zareagowało tą emotką na tę wiadomość
        // To jest kluczowe - nie bierzemy pola .Count z obiektu, tylko liczymy wiersze w bazie
        int totalCount = _context.Reactions
            .Count(r => r.MessageId == messageId && r.Emoji == emoji);

        // 5. Wyślij nową liczbę do wszystkich podłączonych klientów
        await Clients.All.SendAsync("UpdateReaction", messageId, emoji, totalCount);
    }
}
