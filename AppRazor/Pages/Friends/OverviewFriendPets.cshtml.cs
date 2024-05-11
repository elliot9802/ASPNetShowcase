using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;

namespace AppRazor.Pages.Friends
{
    public class OverviewFriendPetsModel : PageModel
    {
        private readonly ILogger<OverviewFriendPetsModel> _logger;
        private IFriendsService _service;

        public List<csFriendPetByCity> FriendsAndPetsByCity { get; set; } = new List<csFriendPetByCity>();
        public string Country { get; set; }

        public OverviewFriendPetsModel(ILogger<OverviewFriendPetsModel> logger, IFriendsService service)
        {
            _logger = logger;
            _service = service;
        }

        public async Task<IActionResult> OnGet(string country = "Finland")
        {
            Country = country;
            try
            {
                var allFriends = (await _service.ReadFriendsAsync(null, true, false, null, 0, 1000))
                                .Concat(await _service.ReadFriendsAsync(null, false, false, null, 0, 1000))
                                .ToList();
                var allPets = (await _service.ReadPetsAsync(null, true, false, null, 0, 1000))
                            .Concat(await _service.ReadPetsAsync(null, false, false, null, 0, 1000))
                            .ToList();
                var info = await _service.InfoAsync;
                if (info == null)
                {
                    _logger.LogError("Service returned null.");
                    return RedirectToPage("./Error");
                }

                var citiesInCountry = info.Friends
                    .Where(f => f.Country == country)
                    .Select(f => f.City)
                    .Distinct()
                    .ToList();

                if (country == "Null Country" && !citiesInCountry.Contains("Null City"))
                {
                    citiesInCountry.Add("Null City");
                }

                FriendsAndPetsByCity = citiesInCountry
                    .Where(city => country == "Null Country" || city != null)
                    .Select(city =>
                    {
                        int nrFriends, nrPets;

                        if (city == "Null City")
                        {
                            nrFriends = allFriends.Count(f => f.Address == null || (f.Address.City == null && f.Address.Country == null));
                            nrPets = allPets.Count(p => p.Friend.Address == null || (p.Friend.Address.City == null && p.Friend.Address.Country == null));
                        }
                        else
                        {
                            nrFriends = allFriends.Where(f => f.Address != null && f.Address.City == city && f.Address.Country == country).Count();
                            nrPets = info.Pets.Where(p => p.City == city && p.Country == country).Sum(p => p.NrPets);
                        }

                        return new csFriendPetByCity()
                        {
                            City = city ?? "Null City",
                            NrFriends = nrFriends,
                            NrPets = nrPets
                        };
                    }).ToList();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in OverviewFriendPets.");
                return RedirectToPage("./Error");
            }
        }

        public class csFriendPetByCity
        {
            public string City { get; set; }
            public int NrFriends { get; set; }
            public int NrPets { get; set; }
        }
    }
}
