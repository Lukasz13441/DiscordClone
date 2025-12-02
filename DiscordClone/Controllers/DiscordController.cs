using Microsoft.AspNetCore.Mvc;

namespace DiscordClone.Controllers
{
    public class DiscordController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
