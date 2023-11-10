using System.Diagnostics;
using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Web.Features.Shared;
using Tailors.Web.Helper;

namespace Tailors.Web.Features.Feed;

[Authorize]
public class FeedController : Controller
{
    private const int PageSize = 20;
    private readonly ShowFeedUseCase _showFeedUseCase;
    private readonly ITweedViewModelFactory _tweedViewModelFactory;
    private readonly UserManager<AppUser> _userManager;

    public FeedController(ShowFeedUseCase showFeedUseCase, UserManager<AppUser> userManager,
        ITweedViewModelFactory tweedViewModelFactory)
    {
        _showFeedUseCase = showFeedUseCase;
        _userManager = userManager;
        _tweedViewModelFactory = tweedViewModelFactory;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = _userManager.GetUserId(User)!;

        var feedViewModel = await BuildFeedViewModel(0, currentUserId);
        var viewModel = new IndexViewModel
        {
            Feed = feedViewModel
        };
        return View(viewModel);
    }

    public async Task<IActionResult> Feed(int page = 0)
    {
        var currentUserId = _userManager.GetUserId(User)!;

        var viewModel = await BuildFeedViewModel(page, currentUserId);
        return PartialView("_Feed", viewModel);
    }

    public async Task<IActionResult> NewTweedsNotification(DateTime since)
    {
        if (!Request.IsHtmx())
            throw new Exception("HTMX request expected");

        var currentUserId = _userManager.GetUserId(User)!;
        var feed = await _showFeedUseCase.GetFeed(currentUserId, 0, PageSize);
        var mostRecentFeedItem = feed.First();

        if (mostRecentFeedItem.CreatedAt > since)
            return PartialView(new NewTweedsNotificationViewModel { NewTweedsAvailable = true });

        return PartialView(new NewTweedsNotificationViewModel());
    }

    private async Task<FeedViewModel> BuildFeedViewModel(int page, string currentUserId)
    {
        var feed = await _showFeedUseCase.GetFeed(currentUserId, page, PageSize);
        var viewModel = new FeedViewModel
        {
            Page = page,
            Tweeds = await _tweedViewModelFactory.Create(feed),
            NextPageExists = feed.Count == PageSize
        };
        return viewModel;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
