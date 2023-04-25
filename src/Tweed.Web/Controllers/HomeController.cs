using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Helper;
using Tweed.Web.Views.Home;
using Tweed.Web.Views.Shared;

namespace Tweed.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<TweedIdentityUser> _userManager;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly ITweedUserQueries _tweedUserQueries;

    public HomeController(ITweedQueries tweedQueries, UserManager<TweedIdentityUser> userManager,
        IViewModelFactory viewModelFactory, ITweedUserQueries tweedUserQueries)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
        _viewModelFactory = viewModelFactory;
        _tweedUserQueries = tweedUserQueries;
    }

    public async Task<IActionResult> Index()
    {
        var currentIdentityUser = await _userManager.GetUserAsync(User)!;
        var currentTweedUser = await _tweedUserQueries.FindByIdentityUserId(currentIdentityUser.Id!);
        var feed = await _tweedQueries.GetFeed(currentTweedUser.Id!);

        List<TweedViewModel> tweedViewModels = new();
        foreach (var tweed in feed)
        {
            var tweedViewModel = await _viewModelFactory.BuildTweedViewModel(tweed);
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
