using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Views.Home;
using Tweed.Web.Views.Shared;

namespace Tweed.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(ITweedQueries tweedQueries, UserManager<AppUser> userManager)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = _userManager.GetUserId(User)!;
        var latestTweeds = await _tweedQueries.GetFeed(currentUserId);

        List<TweedViewModel> tweedViewModels = new();
        foreach (var tweed in latestTweeds)
        {
            var author = await _userManager.FindByIdAsync(tweed.AuthorId);
            var likesCount = await _tweedQueries.GetLikesCount(tweed.Id);
            var tweedViewModel = ViewModelFactory.BuildTweedViewModel(tweed, likesCount, author, currentUserId);
            tweedViewModels.Add(tweedViewModel);
        }

        var viewModel = new IndexViewModel
        {
            Tweeds = tweedViewModels
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
            { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
