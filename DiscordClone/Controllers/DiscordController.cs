using DiscordClone.Data;
using DiscordClone.Models;
using DiscordClone.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            ViewBag.Servers = _ServerService.GetUserServers(userId);
            ViewBag.Friends = _FriendsService.GetUserFriends(userId);
            return View();
        }     
        
        public IActionResult Server(int Id)
        {
            var userId = GetUserId();
            //Channel
            var ChannelId = _ChannelService.GetFirstChannel(Id);
            ViewBag.Id = ChannelId;
            ViewBag.UserProfile = _FriendsService.GetUserProfile(userId);
            ViewBag.Server = _ServerService.GetServerById(Id);
            ViewBag.Servers = _ServerService.GetUserServers(userId);
            ViewBag.Chanels = _ChannelService.GetChanels(Id);
            ViewBag.Messages = _ChannelService.GetChannelMessages(ChannelId);


            return View("Index");
        }

        public IActionResult CreateNewServer()
        {
            return View();
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

        public IActionResult Accept(int FriendId)
        {
            _FriendsService.AcceptFriendship(GetUserId(), FriendId);
            return RedirectToAction("Index");
        }


        public IActionResult AddChanel(int Id)
        {
            ViewBag.Id = Id;
            return View();
        }

        [HttpPost]
        public IActionResult AddChanelForm(Channel model)
        {
            _ChannelService.CreateChannel((int)model.ServerId, model);
            return RedirectToAction("Server", new { Id = model.ServerId });
        }

        public IActionResult SendMessage(int Id)
        {
            var server = _ChannelService.GetServerFromChannelId(Id);
            var userId = GetUserId();
            ViewBag.Id = Id;
            ViewBag.UserProfile = _FriendsService.GetUserProfile(userId);
            ViewBag.Server = _ServerService.GetServerById(server.Id);
            ViewBag.Servers = _ServerService.GetUserServers(userId);
            ViewBag.Chanels = _ChannelService.GetChanels(server.Id);

            ViewBag.Messages = _ChannelService.GetChannelMessages(Id);


            return View("Index");
        }



    }
}
