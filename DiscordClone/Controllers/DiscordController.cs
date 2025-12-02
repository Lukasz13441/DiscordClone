using DiscordClone.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DiscordClone.Models;


namespace DiscordClone.Controllers
{
    public class DiscordController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscordController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
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
                    return View(new List<Server>());
                }

                var servers = _context.Servers
                    .Where(s => s.OwnerId == userProfile.Id || _context.ServerMembers.Any(sm => sm.ServerId == s.Id && sm.UserId == userProfile.Id))
                    .ToList();

                return View(servers);
            }

            return View(new List<Server>());
        }

    }
}
