using System.Diagnostics;
using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailors.Domain.TweedAggregate;
using Tailors.Web.Features.Shared;
using Tailors.Web.Helper;

namespace Tailors.Web.Features.Feed;

[Authorize]
public class FeedController(ShowFeedUseCase showFeedUseCase,
        TweedViewModelFactory tweedViewModelFactory)
    : Controller
{
    private const int PageSize = 20;

    public async Task<IActionResult> Index()
    {
        var currentUserId = User.GetId();

        var feedViewModel = await BuildFeedViewModel(0, currentUserId);
        var viewModel = new IndexViewModel
        {
            Feed = feedViewModel
        };
        return View(viewModel);
    }

    public async Task<IActionResult> Feed(int page = 0)
    {
        var currentUserId = User.GetId();

        var viewModel = await BuildFeedViewModel(page, currentUserId);
        return PartialView("_Feed", viewModel);
    }

    public async Task<IActionResult> NewTweedsNotification(DateTime since)
    {
        if (!Request.IsHtmx())
            throw new Exception("HTMX request expected");

        var currentUserId = User.GetId();
        var feed = await showFeedUseCase.GetFeed(currentUserId, 0, PageSize);
        var mostRecentFeedItem = feed.First();

        if (mostRecentFeedItem.CreatedAt > since)
            return PartialView(new NewTweedsNotificationViewModel { NewTweedsAvailable = true });

        return PartialView(new NewTweedsNotificationViewModel());
    }

    private async Task<FeedViewModel> BuildFeedViewModel(int page, string currentUserId)
    {
        var feed = await showFeedUseCase.GetFeed(currentUserId, page, PageSize);
        var viewModel = new FeedViewModel
        {
            Page = page,
            Tweeds = await tweedViewModelFactory.Create(feed, currentUserId),
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
