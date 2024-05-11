using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Services;

namespace AppRazor.Pages.Friends;
public class OverviewByCountryModel : PageModel
{
    private readonly ILogger<OverviewByCountryModel> _logger;
    private IFriendsService _service;

    public List<csFriendsByCountry> friendsByCountry = new List<csFriendsByCountry>();
    public OverviewByCountryModel(ILogger<OverviewByCountryModel> logger, IFriendsService service)
    {
        _logger = logger;
        _service = service;
    }

    public async Task OnGet()
    {
        var allFriends = (await _service.ReadFriendsAsync(null, true, false, null, 0, 1000))
                                .Concat(await _service.ReadFriendsAsync(null, false, false, null, 0, 1000))
                                .ToList();
        var info = await _service.InfoAsync;

        friendsByCountry = info.Friends
                    .Where(f => f.Country != null)
                    .GroupBy(f => f.Country)
                    .Select(group => new csFriendsByCountry
                    {
                        Country = group.Key,
                        Cities = group.Where(g => g.City != null).ToList()
                    })
                    .ToList();

        // Counting friends with no assigned country
        int nrFriendsNoCountry = allFriends.Count(f => f.Address == null || f.Address.Country == null);
        friendsByCountry.Add(new csFriendsByCountry
        {
            Country = "Null Country",
            Cities = new List<gstusrInfoFriendsDto> { new gstusrInfoFriendsDto { NrFriends = nrFriendsNoCountry } }
        });
    }



    public class csFriendsByCountry
    {
        public string Country { get; set; }
        public List<gstusrInfoFriendsDto> Cities { get; set; }
    }

}