using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Views.Profile;
using Tweed.Web.Views.Shared;

namespace Tweed.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ITweedQueries _tweedQueries;
    private readonly UserManager<AppUser> _userManager;

    public ProfileController(ITweedQueries tweedQueries, UserManager<AppUser> userManager)
    {
        _tweedQueries = tweedQueries;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var userTweeds = await _tweedQueries.GetTweedsForUser(userId);
        var currentUserId = _userManager.GetUserId(User);

        List<TweedViewModel> tweedViewModels = new();
        foreach (var tweed in userTweeds)
        {
            var author = await _userManager.FindByIdAsync(tweed.AuthorId);
            var tweedViewModel = ViewModelFactory.BuildTweedViewModel(tweed, author, currentUserId);
            tweedViewModels.Add(tweedViewModel);
        }

        var viewModel = new IndexViewModel
        {
            UserName = user.UserName,
            Tweeds = tweedViewModels
        };

        return View(viewModel);
    }
}
