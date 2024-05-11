using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Services;

namespace AppRazor.Pages.Home;
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private IFriendsService _service;

    public IndexModel(ILogger<IndexModel> logger, IFriendsService service)
    {
        _logger = logger;
        _service = service;
    }

    public async Task OnGet()
    {
    }
}