using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using Services;
using System.ComponentModel.DataAnnotations;

namespace AppRazor.Pages.Friends
{
    public class EditAddToFriendModel : PageModel
    {
        private readonly ILogger<EditAddToFriendModel> _logger;
        private IFriendsService _service;
        private csSeedGenerator _seeder = new csSeedGenerator();

        public EditAddToFriendModel(ILogger<EditAddToFriendModel> logger, IFriendsService service)
        {
            _logger = logger;
            _service = service;
        }

        [BindProperty]
        public csFriendIM FriendIM { get; set; }
        [BindProperty]
        public csPetIM PetIM { get; set; }
        [BindProperty]
        public csQuoteIM QuoteIM { get; set; }
        public IFriend Friend { get; set; } = null;

        #region HTTP Requests
        public async Task<IActionResult> OnGet(Guid id)
        {
            var f = await _service.ReadFriendAsync(null, id, false);
            FriendIM = new csFriendIM(f);

            if (Guid.TryParse(Request.Query["id"], out Guid _id))
            {
                FriendIM = new csFriendIM(await _service.ReadFriendAsync(null, _id, false));
            }
            else
            {
                FriendIM = new csFriendIM();
                FriendIM.StatusIM = enStatusIM.Inserted;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAddPet()
        {
            if (FriendIM.FriendId == Guid.Empty)
            {
                return RedirectToPage("/Error");
            }

            //Reload Friend from the database
            Friend = await _service.ReadFriendAsync(null, FriendIM.FriendId, false);
            if (Friend == null)
            {
                return RedirectToPage("/Error");
            }

            //Create a new Pet in the database
            var dtoPet = new csPetCUdto()
            {
                Name = PetIM.Name,
                Kind = PetIM.Kind,
                Mood = PetIM.Mood,
                FriendId = Friend.FriendId
            };
            try
            {
                var newPet = await _service.CreatePetAsync(null, dtoPet);

                //Update the Friend by dto
                var dtoFriend = new csFriendCUdto(Friend);
                dtoFriend.PetsId = dtoFriend.PetsId ?? new List<Guid>();
                dtoFriend.PetsId.Add(newPet.PetId);
                Friend = await _service.UpdateFriendAsync(null, dtoFriend);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToPage("/Error");
            }
            return RedirectToPage("./ViewEditFriend", new { id = FriendIM.FriendId });
        }

        public async Task<IActionResult> OnPostAddQuote()
        {
            if (FriendIM.FriendId == Guid.Empty)
            {
                return RedirectToPage("/Error");
            }

            //Reload Friend from the database
            Friend = await _service.ReadFriendAsync(null, FriendIM.FriendId, false);
            if (Friend == null)
            {
                return RedirectToPage("/Error");
            }

            if (string.IsNullOrWhiteSpace(QuoteIM.Quote) || string.IsNullOrWhiteSpace(QuoteIM.Author))
            {
                ModelState.AddModelError("", "Author and Quote are required.");
                return RedirectToPage("./ViewEditFriend", new { id = FriendIM.FriendId });
            }
            //Create a new Quote
            var dtoQuote = new csQuoteCUdto()
            {
                Quote = QuoteIM.Quote,
                Author = QuoteIM.Author,
            };

            try
            {
                var newQuote = await _service.CreateQuoteAsync(null, dtoQuote);

                var dtoFriend = new csFriendCUdto(Friend);
                dtoFriend.QuotesId = dtoFriend.QuotesId ?? new List<Guid>();
                dtoFriend.QuotesId.Add(newQuote.QuoteId);
                Friend = await _service.UpdateFriendAsync(null, dtoFriend);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToPage("/Error");
            }

            return RedirectToPage("./ViewEditFriend", new { id = FriendIM.FriendId });
        }

        public async Task<IActionResult> OnPostEditFriend()
        {
            //Reload Friend from the database
            var model = await _service.ReadFriendAsync(null, FriendIM.FriendId, false);

            if (model == null)
            {
                // Handle the scenario where the friend is not found
                return RedirectToPage("/Error");
            }

            model = FriendIM.UpdateModel(model);

            //Update the Friend by dto
            var dtoFriend = new csFriendCUdto(model);
            model = await _service.UpdateFriendAsync(null, dtoFriend);

            // Check if FriendIM.Address is not null before proceeding with address update
            if (FriendIM.Address != null)
            {
                var adr = await _service.ReadAddressAsync(null, FriendIM.Address.AddressId, true);
                if (adr != null)
                {
                    adr = FriendIM.Address.UpdateModel(adr);
                    var adrdto = new csAddressCUdto(adr);
                    await _service.UpdateAddressAsync(null, adrdto);
                }
            }
            model = await _service.ReadFriendAsync(null, FriendIM.FriendId, false);
            FriendIM = new csFriendIM(model);
            return RedirectToPage("./ViewEditFriend", new { id = FriendIM.FriendId });
        }

        public async Task<IActionResult> OnPostEditPet(Guid petId)
        {
            if (FriendIM == null || FriendIM.FriendId == Guid.Empty)
            {
                _logger.LogError("FriendIM is null or FriendId is empty");
                return RedirectToPage("/Error");
            }

            Friend = await _service.ReadFriendAsync(null, FriendIM.FriendId, false);
            if (Friend == null)
            {
                _logger.LogError($"No friend found with FriendId: {FriendIM.FriendId}");
                return RedirectToPage("/Error");
            }

            if (Friend.Pets == null || !Friend.Pets.Any())
            {
                _logger.LogError($"No pets found for friend with FriendId: {FriendIM.FriendId}");
                return RedirectToPage("/Error");
            }

            var petToUpdate = Friend.Pets.FirstOrDefault(p => p.PetId == petId);
            if (petToUpdate == null)
            {
                _logger.LogError($"No pet found with PetId: {petId}");
                return RedirectToPage("/Error");
            }

            petToUpdate = PetIM.UpdateModel(petToUpdate);

            var dtoPet = new csPetCUdto(petToUpdate);
            _logger.LogInformation($"Updating Pet - Name: {PetIM.Name}, Kind: {PetIM.Kind}, Mood: {PetIM.Mood}");
            if (string.IsNullOrEmpty(PetIM.Name))
            {
                _logger.LogError("Pet name is null or empty");
                return RedirectToPage("/Error");
            }

            await _service.UpdatePetAsync(null, dtoPet);

            // After processing, clear the model state for fields that need to be repopulated from the model
            ModelState.Remove("Name");
            ModelState.Remove("Kind");
            ModelState.Remove("Mood");


            // Repopulate FriendIM to update the page with the latest data
            var updatedFriend = await _service.ReadFriendAsync(null, FriendIM.FriendId, false);
            if (updatedFriend != null)
            {
                FriendIM = new csFriendIM(updatedFriend);
            }
            return RedirectToPage("./ViewEditFriend", new { id = FriendIM.FriendId });
        }

        public async Task<IActionResult> OnPostEditQuote(Guid quoteId)
        {
            if (FriendIM == null || FriendIM.FriendId == Guid.Empty)
            {
                _logger.LogError("FriendIM is null or FriendId is empty");
                return RedirectToPage("/Error");
            }

            Friend = await _service.ReadFriendAsync(null, FriendIM.FriendId, false);
            if (Friend == null)
            {
                _logger.LogError($"No friend found with FriendId: {FriendIM.FriendId}");
                return RedirectToPage("/Error");
            }

            if (Friend.Quotes == null || !Friend.Quotes.Any())
            {
                _logger.LogError($"No Quotes found for friend with FriendId: {FriendIM.FriendId}");
                return RedirectToPage("/Error");
            }

            var quoteToUpdate = Friend.Quotes.FirstOrDefault(q => q.QuoteId == quoteId);
            if (quoteToUpdate == null)
            {
                _logger.LogError($"No Quote found with QuoteId: {quoteId}");
                return RedirectToPage("/Error");
            }

            quoteToUpdate = QuoteIM.UpdateModel(quoteToUpdate);

            var dtoQuote = new csQuoteCUdto(quoteToUpdate);
            _logger.LogInformation($"Updating Quote - Author: {QuoteIM.Author}, Quote: {QuoteIM.Quote}");


            await _service.UpdateQuoteAsync(null, dtoQuote);

            // Repopulate QuoteIM to update the page with the latest data
            var updatedFriend = await _service.ReadFriendAsync(null, FriendIM.FriendId, false);
            if (updatedFriend != null)
            {
                FriendIM = new csFriendIM(updatedFriend);
            }
            return RedirectToPage("./ViewEditFriend", new { id = FriendIM.FriendId });
        }

        public async Task<IActionResult> OnPostDeletePet(Guid petId)
        {
            if (FriendIM.FriendId == Guid.Empty)
            {
                return RedirectToPage("/Error");
            }

            //Reload Friend from the database
            Friend = await _service.ReadFriendAsync(null, FriendIM.FriendId, false);
            if (Friend == null)
            {
                return RedirectToPage("/Error");
            }

            //Delete the Pet in the database
            await _service.DeletePetAsync(null, petId);

            return RedirectToPage(new { id = FriendIM.FriendId });
        }

        public async Task<IActionResult> OnPostDeleteQuote(Guid quoteId)
        {
            if (FriendIM.FriendId == Guid.Empty)
            {
                return RedirectToPage("/Error");
            }

            //Reload Friend from the database
            Friend = await _service.ReadFriendAsync(null, FriendIM.FriendId, false);
            if (Friend == null)
            {
                return RedirectToPage("/Error");
            }

            //Delete the Quote in the database
            await _service.DeleteQuoteAsync(null, quoteId);

            return RedirectToPage(new { id = FriendIM.FriendId });
        }

        #endregion

        #region Input models
        public enum enStatusIM { Unknown, Unchanged, Inserted, Modified, Deleted }

        public class csAddressIM
        {
            public enStatusIM StatusIM { get; set; }

            public Guid AddressId { get; set; }

            [Required(ErrorMessage = "You must enter an StreetAddress")]
            public string? StreetAddress { get; set; }

            [Required(ErrorMessage = "You must enter a ZipCode")]
            public int ZipCode { get; set; }

            [Required(ErrorMessage = "You must enter an Country")]
            public string? Country { get; set; }

            public string? City { get; set; }

            public IAddress UpdateModel(IAddress model)
            {
                model.AddressId = AddressId;
                model.StreetAddress = StreetAddress;
                model.ZipCode = ZipCode;
                model.Country = Country;
                model.City = City;
                return model;
            }

            public csAddressIM() { }
            public csAddressIM(IAddress model)
            {
                StatusIM = enStatusIM.Unchanged;
                AddressId = model.AddressId;
                StreetAddress = model.StreetAddress;
                ZipCode = model.ZipCode;
                Country = model.Country;
                City = model.City;
            }
        }
        public class csPetIM
        {
            public enStatusIM StatusIM { get; set; }
            public Guid PetId { get; set; }

            [Required(ErrorMessage = "You must provide a pet name")]
            public string Name { get; set; }

            [Required(ErrorMessage = "You must select an Animal Mood")]
            public enAnimalMood Mood { get; set; }

            [Required(ErrorMessage = "You must select an AnimalKind")]
            public enAnimalKind Kind { get; set; }

            public IPet UpdateModel(IPet model)
            {
                model.PetId = PetId;
                model.Name = Name;
                model.Mood = Mood;
                model.Kind = Kind;
                return model;
            }

            public csPetIM() { }

            public csPetIM(IPet model)
            {
                StatusIM = enStatusIM.Unchanged;
                PetId = model.PetId;
                Name = model.Name;
                Kind = model.Kind;
                Mood = model.Mood;
            }
        }
        public class csQuoteIM
        {
            public enStatusIM StatusIM { get; set; }

            public Guid QuoteId { get; set; }

            [Required(ErrorMessage = "You must provide an Author")]
            public string Author { get; set; }

            [Required(ErrorMessage = "You must provide a Quote")]
            public string Quote { get; set; }
            public IQuote UpdateModel(IQuote model)
            {
                model.Quote = Quote;
                model.Author = Author;
                return model;
            }
            public csQuoteIM() { }
            public csQuoteIM(IQuote model)
            {
                StatusIM = enStatusIM.Unchanged;
                QuoteId = model.QuoteId;
                Quote = model.Quote;
                Author = model.Author;
            }
        }
        public class csFriendIM
        {
            public enStatusIM StatusIM { get; set; }

            public Guid FriendId { get; set; }

            [Required(ErrorMessage = "You must provide a first name")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "You must provide a last name")]
            public string LastName { get; set; }
            public string Email { get; set; }
            public DateTime? Birthday { get; set; }
            public csAddressIM Address { get; set; }
            public List<csPetIM> Pets { get; set; } = new List<csPetIM>();
            public List<csQuoteIM> Quotes { get; set; } = new List<csQuoteIM>();

            public csFriendIM() { }

            public IFriend UpdateModel(IFriend model)
            {
                model.FriendId = FriendId;
                model.FirstName = FirstName;
                model.LastName = LastName;
                model.Email = Email;
                model.Birthday = Birthday;
                return model;
            }

            public csFriendIM(IFriend model)
            {
                StatusIM = enStatusIM.Unchanged;
                FriendId = model.FriendId;
                FirstName = model.FirstName;
                LastName = model.LastName;
                Email = model.Email;
                Birthday = model.Birthday;

                Address = model.Address != null ? new csAddressIM(model.Address) : null;
                Pets = model.Pets?.Select(p => new csPetIM(p)).ToList();
                Quotes = model.Quotes?.Select(q => new csQuoteIM(q)).ToList();
            }
        }
        #endregion
    }
}