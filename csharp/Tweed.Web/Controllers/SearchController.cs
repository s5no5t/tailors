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
    private readonly ITweedQueries _tweedQueries;

    public SearchController(IAppUserQueries appUserQueries, ITweedQueries tweedQueries)
    {
        _appUserQueries = appUserQueries;
        _tweedQueries = tweedQueries;
    }

    public async Task<IActionResult> Index()
    {
        IndexViewModel viewModel = new();
        return View(viewModel);
    }

    public async Task<IActionResult> Results(
        [FromQuery] [Required] [StringLength(50, MinimumLength = 3)] [RegularExpression(@"^[\w\s]*$")]
        string term)
    {
        if (!ModelState.IsValid)
            return View("Index", new IndexViewModel
            {
                Term = term
            });

        var users = await _appUserQueries.Search(term);
        var tweeds = await _tweedQueries.Search(term);
        IndexViewModel viewModel = new()
        {
            Term = term,
            FoundUsers = users.Select(u => new UserViewModel
            {
                UserId = u.Id,
                UserName = u.UserName
            }).ToList(),
            FoundTweeds = tweeds.Select(t => new TweedViewModel
            {
                TweedId = t.Id,
                Text = t.Text
            }).ToList()
        };
        return View("index", viewModel);
    }
}

