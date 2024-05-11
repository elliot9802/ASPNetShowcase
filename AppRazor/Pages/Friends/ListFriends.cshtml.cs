using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Services;

namespace AppRazor.Pages.Friends
{
    public class ListFriendsModel : PageModel
    {
        private readonly ILogger<ListFriendsModel> _logger;
        private IFriendsService _service;

        public List<IFriend> FriendsList { get; set; }
        public Dictionary<string, List<IFriend>> GroupedFriends { get; set; }

        public ListFriendsModel(ILogger<ListFriendsModel> logger, IFriendsService service)
        {
            _logger = logger;
            _service = service;
        }
        public async Task OnGet()
        {
            var seededFriends = await _service.ReadFriendsAsync(null, true, false, null, 0, 1000);
            var nonSeededFriends = await _service.ReadFriendsAsync(null, false, false, null, 0, 1000);

            FriendsList = seededFriends.Concat(nonSeededFriends).ToList();

            GroupedFriends = FriendsList
                .Where(f => f.Address != null)
                .GroupBy(f => f.Address.Country)
                .ToDictionary(g => g.Key, g => g.ToList());

            var friendsWithNoAddress = FriendsList.Where(f => f.Address == null).ToList();
            if (friendsWithNoAddress.Any())
            {
                GroupedFriends.Add("Null Country", friendsWithNoAddress);
            }
        }
    }
}
