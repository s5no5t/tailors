using System.ComponentModel.DataAnnotations;
using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Web.Helper;

namespace Tailors.Web.Features.Search;

public enum SearchKind
{
    Users = 0,
    Tweeds = 1
}

[Authorize]
public class SearchController(
    ITweedRepository tweedRepository,
    IUserRepository userRepository,
    TweedViewModelFactory tweedViewModelFactory) : Controller
{
    public async Task<IActionResult> Results(
        [FromQuery] [Required] [StringLength(50, MinimumLength = 3)] [RegularExpression(@"^[\w\s]*$")]
        string term,
        [FromQuery] SearchKind? searchKind)
    {
        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx())
                return PartialView("Results", new ResultsViewModel(term, searchKind ?? SearchKind.Users, [], []));
            return View("Results", new ResultsViewModel(term, searchKind ?? SearchKind.Users, [], []));
        }

        var currentUserId = User.GetId();
        var users = await userRepository.Search(term);
        var tweeds = await tweedRepository.Search(term);
        ResultsViewModel viewModel = new(
            term,
            searchKind ?? SearchKind.Users,
            users.Select(u => new UserViewModel(u.Id!, u.UserName)).ToList(),
            await tweedViewModelFactory.Create(tweeds, currentUserId)
        );

        if (Request.IsHtmx())
            return PartialView("Results", viewModel);
        return View("Results", viewModel);
    }
}
