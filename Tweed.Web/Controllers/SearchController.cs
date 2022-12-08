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

    public async Task<IActionResult> Index()
    {
        IndexViewModel viewModel = new();
        return View(viewModel);
    }
}




