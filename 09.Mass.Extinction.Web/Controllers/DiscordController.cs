namespace _09.Mass.Extinction.Web.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Data;
    using Data.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Authorize(Roles = "DiscordAdmin")]
    public class DiscordController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscordController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Redirect()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Messages()
        {
            IEnumerable<Message> messages = await _context.Messages.ToListAsync();

            return View(messages);
        }
    }
}
