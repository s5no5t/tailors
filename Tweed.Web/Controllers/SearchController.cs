using System.ComponentModel.DataAnnotations;
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

    public async Task<IActionResult> Results([FromQuery] [Required] string term)
    {
        if (!ModelState.IsValid)
            return View("Index");

        var users = await _appUserQueries.Search(term);
        IndexViewModel viewModel = new()
        {
            Term = term,
            FoundUsers = users.Select(u => new UserViewModel
            {
                UserId = u.Id,
                UserName = u.UserName
            }).ToList()
        };
        return View("index", viewModel);
    }
}
