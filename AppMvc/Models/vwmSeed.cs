using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AppMvc.Models
{
    public class vwmSeed
    {
        [BindProperty]
        [Required(ErrorMessage = "You must enter the number of items to seed")]
        public int NrOfItems { get; set; } = 5;

        [BindProperty]
        public bool RemoveSeeds { get; set; } = true;
        [BindProperty]
        public int NrOfFriends { get; set; }
    }
}
