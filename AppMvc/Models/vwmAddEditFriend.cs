using AppMvc.SeidoHelpers;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.ComponentModel.DataAnnotations;

namespace AppMvc.Models
{
    public class vwmAddEditFriend
    {
        public enum enStatusIM { Unknown, Unchanged, Inserted, Modified, Deleted }

        [BindProperty]
        public csFriendIM FriendIM { get; set; }
        [BindProperty]
        public csPetIM PetIM { get; set; }
        [BindProperty]
        public csQuoteIM QuoteIM { get; set; }
        public IFriend Friend { get; set; } = null;

        //For Validation
        public reModelValidationResult ValidationResult { get; set; } = new reModelValidationResult(false, null, null);

        public class csAddressIM
        {
            public enStatusIM StatusIM { get; set; }

            public Guid AddressId { get; set; }

            [Required(ErrorMessage = "You must enter an StreetAddress")]
            public string StreetAddress { get; set; }

            [Required(ErrorMessage = "You must enter a ZipCode")]
            public int ZipCode { get; set; }

            [Required(ErrorMessage = "You must enter an Country")]
            public string Country { get; set; }

            [Required(ErrorMessage = "You must enter an City")]
            public string City { get; set; }

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
    }
}
