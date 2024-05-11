using AppStudies.SeidoHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using Newtonsoft.Json;
using Services;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Linq;
using static Npgsql.PostgresTypes.PostgresCompositeType;

namespace AppRazor.Pages.Friends
{
    public class ViewEditFriendModel : PageModel
    {
        private readonly ILogger<ViewEditFriendModel> _logger;
        private IFriendsService _service;

        public ViewEditFriendModel(ILogger<ViewEditFriendModel> logger, IFriendsService service)
        {
            _logger = logger;
            _service = service;
        }

        public IFriend Friend { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Guid friendId = Guid.Parse(Request.Query["id"]);
            Friend = await _service.ReadFriendAsync(null, friendId, false);

            return Page();
        }
    }
}
