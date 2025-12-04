using DiscordClone.Data;
using DiscordClone.Data.Migrations;
using DiscordClone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


namespace DiscordClone.Controllers
{
    [Authorize]
    public class DiscordController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscordController(ApplicationDbContext context)
        {
            _context = context;
        }

        public string[]  split(string text)
        {
            // Rozdzielamy po znaku '#'
            string[] parts = text.Split('#');

            string username = parts[0];
            string discriminator = parts[1];
            string[] tab = { username , discriminator };
            return tab;
        }

        public List<Server> SerlectServer()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userProfile = _context.UserProfiles
                    .FirstOrDefault(x => x.UserId == id);


                var UserIdServers = _context.ServerMembers
                    .Where(sm => sm.UserId == userProfile.Id);


                if (userProfile == null)
                {
                    return new List<Server>();
                }

                var servers = _context.Servers
                    .Where(s => s.OwnerId == userProfile.Id || _context.ServerMembers.Any(sm => sm.ServerId == s.Id && sm.UserId == userProfile.Id))
                    .ToList();

                return servers;
            }

            return new List<Server>();
        }
        public List<UserProfile> SerlectFriend()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userProfile = _context.UserProfiles
                    .FirstOrDefault(x => x.UserId == id);

                var friendProfiles = _context.Friendships
                     .Where(f => f.UserId == userProfile.Id && f.Status == Status.Accepted)
                     .Join(_context.UserProfiles,
                       f => f.FriendId,
                       u => u.Id,
                       (f, u) => u)
                     .ToList();

                return friendProfiles;
            }

            return new List<UserProfile>();
        }

        public IActionResult Index()
        {
            var servers = SerlectServer();
            ViewBag.Servers = servers;

            var friends = SerlectFriend();
            ViewBag.Friends = friends;
            return View(); 

        }

        public IActionResult ListOfFriends()
        {
            var servers = SerlectServer();
            ViewBag.Servers = servers;

            var friends = SerlectFriend();
            ViewBag.Friends = friends;
            return View(); 

        }

        public IActionResult ManageFriends()
        {
            var servers = SerlectServer();
            ViewBag.Servers = servers;

            var friends = SerlectFriend();
            ViewBag.Friends = friends;

            if (User.Identity?.IsAuthenticated == true)
            {
                var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userProfile = _context.UserProfiles
                    .FirstOrDefault(x => x.UserId == id);

                var pendingFriendships = _context.Friendships
                     .Where(f => f.FriendId == userProfile.Id && f.Status == Status.Pending)
                     .Join(_context.UserProfiles,
                       f => f.UserId,
                       u => u.Id,
                       (f, u) => u)
                     .ToList();

                ViewBag.PendingFriends = pendingFriendships;
            }

            return View();

        }

        public IActionResult CreateNewServer()
        {
            var servers = SerlectServer();
            ViewBag.Servers = servers;

            var friends = SerlectFriend();
            ViewBag.Friends = friends;

            return View(new Server());
        }
        public IActionResult CreateNewServerForm(Server model)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userProfile = _context.UserProfiles
                    .FirstOrDefault(x => x.UserId == id);

                if (userProfile == null)
                {
                    return RedirectToAction("Index");
                }

                _context.Servers.Add(new Server
                {
                    Name = model.Name,
                    OwnerId = userProfile.Id,
                    CreatedAt = DateTime.Now,
                    IconURL = "/images/default_avatar.png"
                });
                _context.SaveChanges();


            }
            return RedirectToAction("CreateNewServer");
        }

        public IActionResult AddFriend(FriendString model)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userProfile = _context.UserProfiles
                    .FirstOrDefault(x => x.UserId == id);

                if (userProfile == null)
                {
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrEmpty(model.Name))
                {
                    return BadRequest("Username is required.");
                }

                string[] NameTag = split(model.Name);

                var FriendId = _context.UserProfiles
                     .FirstOrDefault(f => f.Username == NameTag[0] && f.Tag == int.Parse(NameTag[1]));



                _context.Friendships.Add(new Friendship
                {
                    UserId = userProfile.Id,
                    FriendId = FriendId.Id,
                    Status = Status.Pending
                });
                _context.SaveChanges();



            }
           
        
            return RedirectToAction("Index");
        }

        public IActionResult Accept(int FriendId)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userProfile = _context.UserProfiles
                    .FirstOrDefault(x => x.UserId == id);

                if (userProfile == null)
                {
                    return RedirectToAction("Index");
                }

                var friendship = _context.Friendships
                         .FirstOrDefault(f => f.FriendId == userProfile.Id);

                if (friendship != null)
                {
                    // Update the status
                    friendship.Status = Status.Accepted;
                    _context.Friendships.Add(new Friendship
                    {
                        UserId = userProfile.Id,
                        FriendId = 3,
                        Status = Status.Accepted
                    });

                    // Save changes
                    _context.SaveChanges();

                }

                
                



            }
            return RedirectToAction("Index");
        }
    }

}

