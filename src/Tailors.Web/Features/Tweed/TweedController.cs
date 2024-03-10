using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserLikesAggregate;
using Tailors.Web.Helper;

namespace Tailors.Web.Features.Tweed;

[Authorize]
public class TweedController(
    ITweedRepository tweedRepository,
    TweedViewModelFactory tweedViewModelFactory)
    : Controller
{
    [HttpGet("Tweed/{tweedId}")]
    public async Task<ActionResult> ShowThreadForTweed(string tweedId,
        [FromServices] ThreadUseCase threadUseCase)
    {
        var decodedTweedId = HttpUtility.UrlDecode(tweedId); // ASP.NET Core doesn't auto-decode parameters

        var leadingTweedsResult = await threadUseCase.GetThreadTweedsForTweed(decodedTweedId);
        var replyTweedsResult = await threadUseCase.GetReplyTweedsForTweed(decodedTweedId);

        if (!leadingTweedsResult.TryPickT0(out var leadingTweeds, out _))
            return NotFound();
        if (!replyTweedsResult.TryPickT0(out var replyTweeds, out _))
            return NotFound();

        var currentUserId = User.GetId();

        ShowThreadForTweedViewModel viewModel = new()
        {
            LeadingTweeds = await tweedViewModelFactory.Create(leadingTweeds, currentUserId!, decodedTweedId),
            CreateReplyTweed = new CreateTweedViewModel
            {
                ParentTweedId = decodedTweedId
            },
            ReplyTweeds = await tweedViewModelFactory.Create(replyTweeds, currentUserId!)
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTweedViewModel viewModel,
        [FromServices] CreateTweedUseCase createTweedUseCase,
        [FromServices] INotificationManager notificationManager)
    {
        if (!ModelState.IsValid) return PartialView("_CreateTweed", viewModel);

        var currentUserId = User.GetId();
        var now = DateTime.UtcNow;

        if (viewModel.ParentTweedId is null)
        {
            var tweed = await createTweedUseCase.CreateRootTweed(currentUserId!, viewModel.Text, now);
            return tweed.Match<ActionResult>(
                _ =>
                {
                    notificationManager.AppendSuccess("Tweed Posted");
                    return RedirectToAction("Index", "Feed");
                });
        }
        else
        {
            var tweed = await createTweedUseCase.CreateReplyTweed(currentUserId!, viewModel.Text, now,
                viewModel.ParentTweedId);
            return tweed.Match<ActionResult>(
                _ =>
                {
                    notificationManager.AppendSuccess("Reply Posted");
                    return RedirectToAction("Index", "Feed");
                }
                ,
                _ => BadRequest());
        }
    }

    [HttpPost]
    public async Task<IActionResult> Like(string tweedId, bool isCurrent,
        [FromServices] LikeTweedUseCase likeTweedUseCase)
    {
        var getResult = await tweedRepository.GetById(tweedId);
        if (getResult.TryPickT1(out _, out var tweed))
            return NotFound();

        var currentUserId = User.GetId();
        var now = DateTime.UtcNow;
        await likeTweedUseCase.AddLike(tweedId, currentUserId, now);

        var viewModel = await tweedViewModelFactory.Create(tweed, currentUserId, isCurrent);
        return PartialView("_Tweed", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Unlike(string tweedId, bool isCurrent,
        [FromServices] LikeTweedUseCase likeTweedUseCase)
    {
        var getResult = await tweedRepository.GetById(tweedId);
        if (getResult.TryPickT1(out _, out var tweed))
            return NotFound();

        var currentUserId = User.GetId();
        await likeTweedUseCase.RemoveLike(tweedId, currentUserId);

        var viewModel = await tweedViewModelFactory.Create(tweed, currentUserId, isCurrent);
        return PartialView("_Tweed", viewModel);
    }
}
