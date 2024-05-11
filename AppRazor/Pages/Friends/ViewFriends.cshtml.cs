using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using Services;

namespace AppRazor.Pages.Friends
{
    public class ViewFriendsModel : PageModel
    {
        private readonly ILogger<ViewFriendsModel> _logger;
        private IFriendsService _service;
        private csSeedGenerator _seeder = new csSeedGenerator();

        public ViewFriendsModel(ILogger<ViewFriendsModel> logger, IFriendsService service)
        {
            _logger = logger;
            _service = service;
        }
        public IFriend Friend { get; set; } = null;

        [BindProperty]
        public Guid FriendId { get; set; }
        public List<IFriend> Friends { get; set; }
        public int NrOfFriends => Friends.Count;

        public async Task<IActionResult> OnGet()
        {
            var nonSeededFriends = await _service.ReadFriendsAsync(null, false, false, null, 0, 1000);
            var seededFriends = await _service.ReadFriendsAsync(null, true, false, null, 0, 1000);

            Friends = nonSeededFriends.Concat(seededFriends).ToList();

            //var NrOfFriends = Friends.Count;

            return Page();
        }

        public async Task<IActionResult> OnPostNewFriend()
        {
            Friend = new csFriend().Seed(_seeder);
            var address = new csAddress().Seed(_seeder);

            csAddressCUdto addressDto = new csAddressCUdto(address);
            addressDto.AddressId = null;
            var addressFromDb = await _service.CreateAddressAsync(null, addressDto);

            Friend.Address = addressFromDb;

            var dtoFriend = new csFriendCUdto(Friend)
            {
                FriendId = null,
                AddressId = addressFromDb.AddressId
            };

            var FriendFromDb = await _service.CreateFriendAsync(null, dtoFriend);
            FriendId = FriendFromDb.FriendId;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete()
        {
            if (FriendId == Guid.Empty)
            {
                return RedirectToPage("/Error");
            }
            //Reload Friend from the database
            Friend = await _service.ReadFriendAsync(null, FriendId, false);
            if (Friend == null)
            {
                return RedirectToPage("/Error");
            }
            await _service.DeleteFriendAsync(null, FriendId);

            //Reload Friend from the database
            Friend = await _service.ReadFriendAsync(null, FriendId, false);

            return RedirectToPage();
        }
    }
}
