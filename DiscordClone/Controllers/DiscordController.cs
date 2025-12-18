using DiscordClone.Data;
using DiscordClone.Models;
using DiscordClone.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Channels;
using Channel = DiscordClone.Models.Channel;

namespace DiscordClone.Controllers
{
    [Authorize]
    public class DiscordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FriendsService _FriendsService;
        private readonly ChannelService _ChannelService;
        private readonly ServerService _ServerService;

        public DiscordController(ApplicationDbContext context, FriendsService FriendsService, ChannelService channelService, ServerService serverService)
        {
            _context = context;
            _FriendsService = FriendsService;
            _ChannelService = channelService;
            _ServerService = serverService;
        }

        public string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public IActionResult Index()
        {
            var userId = GetUserId();
            ViewBag.UserProfile = _FriendsService.GetUserProfile(userId);
            ViewBag.Servers = _ServerService.GetUserServers(userId);
            ViewBag.Friends = _FriendsService.GetUserFriends(userId);
            return View();
        }

        public IActionResult ManageServers(int Id)
        {
            var userId = _FriendsService.GetUserIntId(GetUserId());
            var member = _ServerService.GetServerMember(Id, GetUserId());
            ViewBag.Role = _ServerService.ServerRole(member);
            ViewBag.Members = _ServerService.GetServerMembers(Id);
            ViewBag.Server = _ServerService.GetServerById(Id);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ManageServersForm(ServerViewModel model)
        {
            await _ServerService.UpdateServerAsync(model);
            return RedirectToAction("ManageServers", new { Id = model.Id });
        }

        public async Task<IActionResult> Server(int Id)
        {
            var userId = GetUserId();
            var channelId = _ChannelService.GetFirstChannel(Id);

            ViewBag.Id = channelId;
            ViewBag.UserProfile = _FriendsService.GetUserProfile(userId);
            ViewBag.Server = _ServerService.GetServerById(Id);
            ViewBag.Servers = _ServerService.GetUserServers(userId);
            ViewBag.Chanels = _ChannelService.GetChanels(Id);
            ViewBag.VoiceChannels = _ChannelService.GetVoiceChannels(Id); // Add this line

            // Load messages with grouped reactions
            var messages = await LoadMessagesWithReactions(channelId);
            ViewBag.Messages = messages;

            return View("Index");
        }

        public IActionResult CreateNewServer()
        {
            return View();
        }

        public IActionResult UserProfile()
        {
            var model = _FriendsService.GetUserProfile(GetUserId());
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UserProfileForm(UserProfileFile model)
        {
            var userId = _FriendsService.GetUserIntId(GetUserId());

            if (userId == null || model == null)
                return RedirectToAction("UserProfile");

            await _ChannelService.UpdateUserProfileAsync(userId, model);

            return RedirectToAction("UserProfile");
        }

        [HttpPost]
        public IActionResult CreateNewServerForm(Server model)
        {
            _ServerService.CreateServer(GetUserId(), model);
            return RedirectToAction("Index");
        }

        public IActionResult AddFriend()
        {
            var userId = GetUserId();
            ViewBag.UserProfile = _FriendsService.GetUserProfile(userId);
            ViewBag.Servers = _ServerService.GetUserServers(userId);
            ViewBag.Friends = _FriendsService.GetUserFriends(userId);
            ViewBag.PendingFriends = _FriendsService.GetPendingFriendRequests(userId);
            ViewBag.AddFriends = 1;
            return View("Index");
        }

        public IActionResult AddFriendForm(FriendString model)
        {
            _FriendsService.AddFriend(GetUserId(), model.Name);
            return RedirectToAction("Index");
        }

        public IActionResult AcceptFriendship(int Id)
        {
            _FriendsService.AcceptFriendship(GetUserId(), Id);
            return RedirectToAction("Index");
        }
        public IActionResult AddChanel(int Id)
        {
            ViewBag.Id = Id;
            return View();
        }

        [HttpPost]
        public IActionResult AddChanelForm(Channel model, string ChannelType)
        {
            if (string.IsNullOrEmpty(ChannelType))
            {
                ChannelType = "text"; // Default to text channel
            }

            if (ChannelType == "voice")
            {
                // Create Voice Channel using ChannelService
                _ChannelService.CreateVoiceChannel((int)model.ServerId, model.Name);
            }
            else
            {
                // Create Text Channel using existing method
                _ChannelService.CreateChannel((int)model.ServerId, model);
            }

            return RedirectToAction("Server", new { Id = model.ServerId });
        }

        public async Task<IActionResult> SendMessage(int Id)
        {
            var server = _ChannelService.GetServerFromChannelId(Id);
            var userId = GetUserId();

            ViewBag.Id = Id;
            ViewBag.UserProfile = _FriendsService.GetUserProfile(userId);
            ViewBag.Server = _ServerService.GetServerById(server.Id);
            ViewBag.Servers = _ServerService.GetUserServers(userId);
            ViewBag.Chanels = _ChannelService.GetChanels(server.Id);
            ViewBag.VoiceChannels = _ChannelService.GetVoiceChannels(server.Id); // Add this line

            // Load messages with grouped reactions
            var messages = await LoadMessagesWithReactions(Id);
            ViewBag.Messages = messages;

            return View("Index");
        }

        public async Task<IActionResult> ChatWithId(int Id)
        {
            var FriendId = Id;
            var userId = GetUserId();

            //tworzymy relacjie jesli nie istnieje
            if (!_FriendsService.ifChating(_FriendsService.GetUserIntId(GetUserId()), FriendId))
            {
                _FriendsService.CreateFriendshipChannel(_FriendsService.GetUserIntId(GetUserId()), FriendId);
            }

            //pobieramy servery i przyjaciol
            ViewBag.Servers = _ServerService.GetUserServers(userId);
            ViewBag.Friends = _FriendsService.GetUserFriends(userId);
            ViewBag.UserProfile = _FriendsService.GetUserProfile(userId);

            //pobiramy id relacji
            var FriendshipId = _FriendsService.GetFriendshipId(_FriendsService.GetUserIntId(GetUserId()), FriendId);
            //tworzymy kanal jesli nie istnieje
            _ChannelService.CreateFriendshipChannel(FriendshipId);
            //pobieramy id kanalu
            var FriendshipChannelId = _ChannelService.GetFriendshipChannelId(FriendshipId);
            ViewBag.Id = FriendshipChannelId;

            // Load messages with grouped reactions
            var messages = await LoadMessagesWithReactions(FriendshipChannelId);
            ViewBag.Messages = messages;

            return View("Index");
        }

        // For Text Channels (existing functionality)
        public IActionResult AddTextChannel(int serverId, string name)
        {
            if (string.IsNullOrEmpty(name))
                return RedirectToAction("Server", new { Id = serverId });

            _ChannelService.CreateChannel(serverId, new Channel { Name = name });
            return RedirectToAction("Server", new { Id = serverId });
        }

        // For Voice Channels (new functionality)
        public IActionResult AddVoiceChannel(int serverId, string name)
        {
            if (string.IsNullOrEmpty(name))
                return RedirectToAction("Server", new { Id = serverId });

            _ChannelService.CreateVoiceChannel(serverId, name);
            return RedirectToAction("Server", new { Id = serverId });
        }



        // ========================================
        // HELPER METHOD - Load Messages with Grouped Reactions
        // ========================================
        private async Task<List<Message>> LoadMessagesWithReactions(int channelId)
        {
            // Load messages with users
            var messages = await _context.Messages
                .Where(m => m.ChannelId == channelId)
                .Include(m => m.User)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            // For each message, load and group reactions
            foreach (var message in messages)
            {
                // Group reactions by Emoji key and count them
                var groupedReactions = await _context.MessageReactions
                    .Where(r => r.MessageId == message.Id)
                    .GroupBy(r => r.Emoji)
                    .Select(g => new MessageReaction
                    {
                        Emoji = g.Key,           // "1", "2", "3", "4", "5"
                        Count = g.Count(),       // How many users reacted with this emoji
                        MessageId = message.Id
                    })
                    .ToListAsync();

                message.Reactions = groupedReactions;
            }

            return messages;
        }

        public IActionResult joinServer(int Id)
        {
            var userId = GetUserId();
            _ServerService.AddMemberToServer(userId, Id);
            return RedirectToAction("Index");
        }


    }
}