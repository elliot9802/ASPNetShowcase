using AppMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace AppMvc.Controllers
{
    public class SeedController : Controller
    {
        private readonly IFriendsService _service;
        private readonly ILogger<SeedController> _logger;

        public SeedController(IFriendsService service, ILogger<SeedController> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var friendsList = await _service.ReadFriendsAsync(null, true, false, null, 0, 1000);

            var vwm = new vwmSeed() { NrOfFriends = friendsList.Count };

            return View("Seed", vwm);
        }

        [HttpPost]
        public async Task<IActionResult> Seed(vwmSeed vwm)
        {
            if (ModelState.IsValid)
            {
                if (vwm.RemoveSeeds)
                {
                    await _service.RemoveSeedAsync(null, true);
                }
                await _service.SeedAsync(null, vwm.NrOfItems);

                return RedirectToAction("ViewFriends", "Friends");
            }

            return View(vwm);
        }
    }
}
