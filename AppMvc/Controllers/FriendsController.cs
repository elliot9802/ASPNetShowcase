using AppMvc.Models;
using AppMvc.SeidoHelpers;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTO;
using Services;
using System.Diagnostics;

namespace AppMvc.Controllers
{
    public class FriendsController : Controller
    {
        IFriendsService _service;
        ILogger<FriendsController> _logger;
        private csSeedGenerator _seeder = new csSeedGenerator();

        public FriendsController(IFriendsService service, ILogger<FriendsController> logger)
        {
            _service = service;
            _logger = logger;
        }
        public async Task<IActionResult> OverviewByCountry()
        {
            try
            {
                var vwm = new vwmOverviewByCountry();
                var allFriends = (await _service.ReadFriendsAsync(null, true, false, null, 0, 1000))
                                .Concat(await _service.ReadFriendsAsync(null, false, false, null, 0, 1000))
                                .ToList();
                var info = await _service.InfoAsync;
                if (info?.Friends == null)
                {
                    _logger.LogError("No friends information found.");
                    return RedirectToAction("Error");
                }

                var friendsByCountry = info.Friends
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

                vwm.FriendsByCountry = friendsByCountry;

                return View(vwm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving friends by country.");
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> OverviewFriendPets(string country = "Finland")
        {
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
                    return RedirectToAction("Error");
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

                var friendsAndPetsByCity = citiesInCountry
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

                        return new csFriendPetByCity
                        {
                            City = city ?? "Null City",
                            NrFriends = nrFriends,
                            NrPets = nrPets
                        };
                    }).ToList();
                ViewData["Country"] = country;

                var vwm = new vwmOverviewFriendPets
                {
                    FriendsAndPetsByCity = friendsAndPetsByCity,
                    Country = country
                };

                return View(vwm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in OverviewFriendPets.");
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> ListFriends()
        {
            var seededFriends = await _service.ReadFriendsAsync(null, true, false, null, 0, 1000);
            var nonSeededFriends = await _service.ReadFriendsAsync(null, false, false, null, 0, 1000);

            var friendsList = seededFriends.Concat(nonSeededFriends).ToList();

            // Grouping friends with addresses
            var groupedFriends = friendsList
                .Where(f => f.Address != null)
                .GroupBy(f => f.Address.Country)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Adding friends with no addresses under a special group
            var friendsWithNoAddress = friendsList.Where(f => f.Address == null).ToList();
            if (friendsWithNoAddress.Any())
            {
                groupedFriends.Add("Null Country", friendsWithNoAddress);
            }

            var vwm = new vwmListFriends()
            {
                FriendsList = friendsList,
                GroupedFriends = groupedFriends
            };
            return View(vwm);
        }

        public async Task<IActionResult> ViewFriend(Guid id)
        {
            var friend = await _service.ReadFriendAsync(null, id, false);
            if (friend == null)
            {
                return RedirectToAction("Error");
            }

            var vwm = new vwmViewFriend() { Friend = friend };
            return View(vwm);
        }

        #region AddEditFriend View Controller Actions
        public async Task<IActionResult> AddEditFriend(Guid? id)
        {
            var vwm = new vwmAddEditFriend();

            if (id.HasValue)
            {
                var f = await _service.ReadFriendAsync(null, id.Value, false);
                if (f != null)
                {
                    vwm.FriendIM = new vwmAddEditFriend.csFriendIM(f);
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
            else
            {
                vwm.FriendIM = new vwmAddEditFriend.csFriendIM
                {
                    StatusIM = vwmAddEditFriend.enStatusIM.Inserted
                };
            }

            return View(vwm);
        }

        [HttpPost]
        public async Task<IActionResult> EditFriend(vwmAddEditFriend vwm)
        {
            var model = await _service.ReadFriendAsync(null, vwm.FriendIM.FriendId, false);

            if (model == null)
            {
                return RedirectToAction("Error");
            }

            model = vwm.FriendIM.UpdateModel(model);

            var dtoFriend = new csFriendCUdto(model);
            model = await _service.UpdateFriendAsync(null, dtoFriend);

            if (vwm.FriendIM.Address != null)
            {
                var adr = await _service.ReadAddressAsync(null, vwm.FriendIM.Address.AddressId, true);
                if (adr != null)
                {
                    adr = vwm.FriendIM.Address.UpdateModel(adr);
                    var adrdto = new csAddressCUdto(adr);
                    await _service.UpdateAddressAsync(null, adrdto);
                }
            }

            model = await _service.ReadFriendAsync(null, vwm.FriendIM.FriendId, false);
            if (model != null)
            {
                vwm.FriendIM = new vwmAddEditFriend.csFriendIM(model);
            }

            return RedirectToAction("ViewFriend", new { id = vwm.FriendIM.FriendId });
        }

        [HttpPost]
        public async Task<IActionResult> AddPet(vwmAddEditFriend vwm)
        {
            if (vwm.FriendIM.FriendId == Guid.Empty)
            {
                return RedirectToAction("Error");
            }

            vwm.Friend = await _service.ReadFriendAsync(null, vwm.FriendIM.FriendId, false);
            if (vwm.Friend == null)
            {
                return RedirectToAction("Error");
            }

            var dtoPet = new csPetCUdto()
            {
                Name = vwm.PetIM.Name,
                Kind = vwm.PetIM.Kind,
                Mood = vwm.PetIM.Mood,
                FriendId = vwm.Friend.FriendId
            };
            try
            {
                var newPet = await _service.CreatePetAsync(null, dtoPet);

                var dtoFriend = new csFriendCUdto(vwm.Friend);
                dtoFriend.PetsId = dtoFriend.PetsId ?? new List<Guid>();
                dtoFriend.PetsId.Add(newPet.PetId);
                vwm.Friend = await _service.UpdateFriendAsync(null, dtoFriend);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Error");
            }
            return RedirectToAction("ViewFriend", new { id = vwm.FriendIM.FriendId });
        }

        [HttpPost]
        public async Task<IActionResult> EditPet(Guid petId, Guid friendId, vwmAddEditFriend vwm)
        {
            string[] keys = { $"PetIM.Name", $"PetIM.Kind", $"PetIM.Mood" };

            if (!ModelState.IsValidPartially(out reModelValidationResult validationResult, keys))
            {
                vwm.ValidationResult = validationResult;
                return RedirectToAction("AddEditFriend", vwm);
            }

            if (friendId == Guid.Empty)
            {
                _logger.LogError("FriendId is empty");
                return RedirectToAction("Error");
            }

            var friend = await _service.ReadFriendAsync(null, friendId, false);
            if (friend == null)
            {
                _logger.LogError($"No friend found with FriendId: {friendId}");
                return RedirectToAction("Error");
            }

            if (friend.Pets == null || !friend.Pets.Any())
            {
                _logger.LogError($"No Quotes found for friend with FriendId: {friendId}");
                return RedirectToAction("Error");
            }

            var petToUpdate = friend.Pets.FirstOrDefault(p => p.PetId == petId);
            if (petToUpdate == null)
            {
                _logger.LogError($"No Quote found with QuoteId: {petId}");
                return RedirectToAction("Error");
            }

            petToUpdate = vwm.PetIM.UpdateModel(petToUpdate);

            var dtoPet = new csPetCUdto(petToUpdate);
            _logger.LogInformation($"Updating Pet - Name: {vwm.PetIM.Name}, Kind: {vwm.PetIM.Kind}, Mood: {vwm.PetIM.Mood}");

            await _service.UpdatePetAsync(null, dtoPet);

            var updatedFriend = await _service.ReadFriendAsync(null, friendId, false);
            if (updatedFriend != null)
            {
                var updatedFriendIM = new vwmAddEditFriend.csFriendIM(updatedFriend);
            }

            return RedirectToAction("ViewFriend", new { id = friendId });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePet(Guid friendId, Guid petId)
        {
            if (friendId == Guid.Empty || petId == Guid.Empty)
            {
                return RedirectToAction("Error");
            }

            var friend = await _service.ReadFriendAsync(null, friendId, false);
            if (friend == null)
            {
                return RedirectToAction("Error");
            }

            await _service.DeletePetAsync(null, petId);

            return RedirectToAction("AddEditFriend", new { id = friendId });
        }

        [HttpPost]
        public async Task<IActionResult> AddQuote(vwmAddEditFriend vwm)
        {
            if (vwm.FriendIM.FriendId == Guid.Empty)
            {
                return RedirectToAction("Error");
            }

            vwm.Friend = await _service.ReadFriendAsync(null, vwm.FriendIM.FriendId, false);
            if (vwm.Friend == null)
            {
                return RedirectToAction("Error");
            }

            var dtoQuote = new csQuoteCUdto()
            {
                Quote = vwm.QuoteIM.Quote,
                Author = vwm.QuoteIM.Author
            };

            try
            {
                var newQuote = await _service.CreateQuoteAsync(null, dtoQuote);

                var dtoFriend = new csFriendCUdto(vwm.Friend);
                dtoFriend.QuotesId = dtoFriend.QuotesId ?? new List<Guid>();
                dtoFriend.QuotesId.Add(newQuote.QuoteId);
                vwm.Friend = await _service.UpdateFriendAsync(null, dtoFriend);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToPage("/Error");
            }
            return RedirectToAction("ViewFriend", new { id = vwm.FriendIM.FriendId });
        }

        [HttpPost]
        public async Task<IActionResult> EditQuote(Guid quoteId, Guid friendId, vwmAddEditFriend vwm)
        {
            string[] keys = { $"QuoteIM.Author", $"QuoteIM.Quote" };

            if (!ModelState.IsValidPartially(out reModelValidationResult validationResult, keys))
            {
                vwm.ValidationResult = validationResult;
                return RedirectToAction("AddEditFriend", vwm);
            }

            if (friendId == Guid.Empty)
            {
                _logger.LogError("FriendId is empty");
                return RedirectToAction("Error");
            }

            var friend = await _service.ReadFriendAsync(null, friendId, false);
            if (friend == null)
            {
                _logger.LogError($"No friend found with FriendId: {friendId}");
                return RedirectToAction("Error");
            }

            if (friend.Quotes == null || !friend.Quotes.Any())
            {
                _logger.LogError($"No Quotes found for friend with FriendId: {friendId}");
                return RedirectToAction("Error");
            }

            var quoteToUpdate = friend.Quotes.FirstOrDefault(q => q.QuoteId == quoteId);
            if (quoteToUpdate == null)
            {
                _logger.LogError($"No Quote found with QuoteId: {quoteId}");
                return RedirectToAction("Error");
            }

            quoteToUpdate = vwm.QuoteIM.UpdateModel(quoteToUpdate);

            var dtoQuote = new csQuoteCUdto(quoteToUpdate);
            _logger.LogInformation($"Updating Quote - Author: {vwm.QuoteIM.Author}, Quote: {vwm.QuoteIM.Quote}");

            await _service.UpdateQuoteAsync(null, dtoQuote);

            var updatedFriend = await _service.ReadFriendAsync(null, friendId, false);
            if (updatedFriend != null)
            {
                var updatedFriendIM = new vwmAddEditFriend.csFriendIM(updatedFriend);
            }

            return RedirectToAction("ViewFriend", new { id = friendId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuote(Guid friendId, Guid quoteId)
        {
            if (friendId == Guid.Empty || quoteId == Guid.Empty)
            {
                return RedirectToAction("Error");
            }

            var friend = await _service.ReadFriendAsync(null, friendId, false);
            if (friend == null)
            {
                return RedirectToAction("Error");
            }

            await _service.DeleteQuoteAsync(null, quoteId);

            return RedirectToAction("AddEditFriend", new { id = friendId });
        }
        #endregion

        #region ViewFriends View Controller Actions
        public async Task<IActionResult> ViewFriends()
        {
            var seededFriends = await _service.ReadFriendsAsync(null, true, false, null, 0, 1000);
            var nonSeededFriends = await _service.ReadFriendsAsync(null, false, false, null, 0, 1000);

            var friends = seededFriends.Concat(nonSeededFriends).ToList();

            var vwm = new vwmViewFriends()
            {
                Friends = friends
            };
            return View(vwm);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewFriend()
        {
            var friend = new csFriend().Seed(_seeder);
            var address = new csAddress().Seed(_seeder);

            csAddressCUdto addressDto = new csAddressCUdto(address)
            {
                AddressId = null
            };
            var addressFromDb = await _service.CreateAddressAsync(null, addressDto);

            friend.Address = addressFromDb;

            var dtoFriend = new csFriendCUdto(friend)
            {
                FriendId = null,
                AddressId = addressFromDb.AddressId
            };

            await _service.CreateFriendAsync(null, dtoFriend);

            return RedirectToAction("ViewFriends");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFriend(Guid friendId)
        {
            if (friendId == Guid.Empty)
            {
                _logger.LogError("DeleteFriend: friendId is empty");
                return RedirectToAction("Error");
            }

            var friend = await _service.ReadFriendAsync(null, friendId, false);
            if (friend == null)
            {
                _logger.LogError($"DeleteFriend: No friend found with friendId: {friendId}");
                return RedirectToAction("Error");
            }

            await _service.DeleteFriendAsync(null, friendId);

            var checkFriend = await _service.ReadFriendAsync(null, friendId, false);
            if (checkFriend != null)
            {
                _logger.LogError($"DeleteFriend: Failed to delete friend with friendId: {friendId}");
                return RedirectToAction("Error");
            }

            return RedirectToAction("ViewFriends");
        }
        #endregion

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
