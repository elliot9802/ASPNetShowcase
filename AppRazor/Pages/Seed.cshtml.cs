using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;
using System.ComponentModel.DataAnnotations;

namespace AppRazor.Pages
{
    public class SeedModel : PageModel
    {
        //Just like for WebApi
        IFriendsService _service = null;
        ILogger<SeedModel> _logger = null;

        public int NrOfFriends => _nrOfFriends().Result;
        private async Task<int> _nrOfFriends()
        {
            var list = await _service.ReadFriendsAsync(null, true,false, null, 0, 1000);
            return list.Count;
        }

        [BindProperty]
        [Required(ErrorMessage = "You must enter nr of items to seed")]
        public int NrOfItems { get; set; } = 5;

        [BindProperty]
        public bool RemoveSeeds { get; set; } = true;

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                if (RemoveSeeds)
                {
                    await _service.RemoveSeedAsync(null, true);
                }
                await _service.SeedAsync(null, NrOfItems);

                return Redirect($"~/Friends/ViewFriends");
            }
            return Page();
        }

        //Inject services just like in WebApi
        public SeedModel(ILogger<SeedModel> logger, IFriendsService service)
        {
            _service = service;
            _logger = logger;
        }
    }
}
