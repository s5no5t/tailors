using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserLikesAggregate;
using Tailors.Web.Helper;

namespace Tailors.Web.Features.Tweed;

[Authorize]
public class TweedController(ITweedRepository tweedRepository,
        UserManager<AppUser> userManager,
        TweedViewModelFactory tweedViewModelFactory)
    : Controller
{
    [HttpGet("Tweed/{tweedId}")]
    public async Task<ActionResult> ShowThreadForTweed(string tweedId,
        [FromServices] ThreadUseCase threadUseCase)
    {
        var decodedTweedId =
            HttpUtility.UrlDecode(tweedId); // ASP.NET Core doesn't auto-decode parameters

        var threadTweedsResult = await threadUseCase.GetThreadTweedsForTweed(decodedTweedId);
        return await threadTweedsResult.Match<Task<ActionResult>>(
            async tweeds =>
            {
                ShowThreadForTweedViewModel viewModel = new()
                {
                    Tweeds = await tweedViewModelFactory.Create(tweeds, decodedTweedId),
                    CreateReplyTweed = new CreateReplyTweedViewModel
                    {
                        ParentTweedId = decodedTweedId
                    }
                };
                return View(viewModel);
            },
            _ => Task.FromResult<ActionResult>(NotFound()));
    }

    [HttpGet("Tweed/Create")]
    public IActionResult Create()
    {
        CreateViewModel viewModel = new();
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTweedViewModel viewModel,
        [FromServices] CreateTweedUseCase createTweedUseCase,
        [FromServices] INotificationManager notificationManager)
    {
        if (!ModelState.IsValid) return PartialView("_CreateTweed", viewModel);

        var currentUserId = userManager.GetUserId(User);
        var now = DateTime.UtcNow;

        var tweed = await createTweedUseCase.CreateRootTweed(currentUserId!, viewModel.Text, now);
        return tweed.Match<ActionResult>(
            _ =>
            {
                notificationManager.AppendSuccess("Tweed Posted");
                return RedirectToAction("Index", "Feed");
            });
    }

    [HttpPost]
    public async Task<IActionResult> CreateReply(CreateReplyTweedViewModel viewModel,
        [FromServices] CreateTweedUseCase createTweedUseCase,
        [FromServices] INotificationManager notificationManager)
    {
        if (!ModelState.IsValid) return PartialView("_CreateReplyTweed", viewModel);

        var currentUserId = userManager.GetUserId(User);
        var now = DateTime.UtcNow;
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

    [HttpPost]
    public async Task<IActionResult> Like(string tweedId, bool isCurrent,
        [FromServices] LikeTweedUseCase likeTweedUseCase)
    {
        var getResult = await tweedRepository.GetById(tweedId);
        if (getResult.TryPickT1(out _, out var tweed))
            return NotFound();

        var currentUserId = userManager.GetUserId(User)!;
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

        var currentUserId = userManager.GetUserId(User)!;
        await likeTweedUseCase.RemoveLike(tweedId, currentUserId);

        var viewModel = await tweedViewModelFactory.Create(tweed, currentUserId, isCurrent);
        return PartialView("_Tweed", viewModel);
    }
}
