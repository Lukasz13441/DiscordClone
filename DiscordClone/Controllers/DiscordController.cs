using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiscordClone.Controllers
{
    [Authorize]
    public class DiscordController : Controller
    {
        private readonly DiscordService _service;

        public DiscordController(DiscordService service)
        {
            _service = service;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public IActionResult Index()
        {
            var userId = GetUserId();
            ViewBag.Servers = _service.GetUserServers(userId);
            ViewBag.Friends = _service.GetUserFriends(userId);
            return View();
        }

        public IActionResult ListOfFriends()
        {
            var userId = GetUserId();
            ViewBag.Servers = _service.GetUserServers(userId);
            ViewBag.Friends = _service.GetUserFriends(userId);
            return View();
        }

        public IActionResult ManageFriends()
        {
            var userId = GetUserId();
            ViewBag.Servers = _service.GetUserServers(userId);
            ViewBag.Friends = _service.GetUserFriends(userId);
            ViewBag.PendingFriends = _service.GetPendingFriendRequests(userId);
            return View();
        }

        public IActionResult CreateNewServer()
        {
            var userId = GetUserId();
            ViewBag.Servers = _service.GetUserServers(userId);
            ViewBag.Friends = _service.GetUserFriends(userId);

            return View(new Server());
        }

        [HttpPost]
        public IActionResult CreateNewServerForm(Server model)
        {
            _service.CreateServer(GetUserId(), model);
            return RedirectToAction("CreateNewServer");
        }

        public IActionResult AddFriend(FriendString model)
        {
            _service.AddFriend(GetUserId(), model.Name);
            return RedirectToAction("Index");
        }

        public IActionResult Accept(int FriendId)
        {
            _service.AcceptFriendship(GetUserId(), FriendId);
            return RedirectToAction("Index");
        }
        

        public IActionResult Server(int Id)
        {
            var userId = GetUserId();
            ViewBag.Servers = _service.GetUserServers(userId);
            ViewBag.Chanels = _service.GetChanels(Id);
            ViewBag.Server = _service.GetServerById(Id);
            return View("Index");
        }
        public IActionResult AddChanel(int Id)
        {
            var userId = GetUserId();
            ViewBag.Id = Id;
            ViewBag.Servers = _service.GetUserServers(userId);
            ViewBag.Chanels = _service.GetChanels(Id);
            ViewBag.Server = _service.GetServerById(Id);
            return View();
        }

        [HttpPost]
        public IActionResult AddChanelForm(Channel model)
        {
            _service.CreateChannel((int)model.ServerId, model);
            return RedirectToAction("Server",new { Id = model.ServerId}); 
        }
    }
}
