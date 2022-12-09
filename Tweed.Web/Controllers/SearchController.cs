using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tweed.Data;
using Tweed.Web.Views.Search;

namespace Tweed.Web.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly IAppUserQueries _appUserQueries;

    public SearchController(IAppUserQueries appUserQueries)
    {
        _appUserQueries = appUserQueries;
    }

    [HttpGet("{term}")]
    public async Task<IActionResult> Index(string term)
    {
        var users = await _appUserQueries.Search(term);
        IndexViewModel viewModel = new()
        {
            Users = users.Select(u => new UserViewModel
            {
                UserId = u.Id
            }).ToList()
        };
        return View(viewModel);
    }
}
