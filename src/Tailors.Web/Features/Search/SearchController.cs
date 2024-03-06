using System.ComponentModel.DataAnnotations;
using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;

namespace Tailors.Web.Features.Search;

[Authorize]
public class SearchController : Controller
{
    public async Task<IActionResult> Results(
        [FromQuery] [Required] [StringLength(50, MinimumLength = 3)] [RegularExpression(@"^[\w\s]*$")]
        string term,
        [FromServices] ITweedRepository tweedRepository,
        [FromServices] IUserRepository userRepository)
    {
        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx())
                return PartialView("Results", new ResultsViewModel(term, [], []));
            return View("Results", new ResultsViewModel(term, [], []));
        }

        var users = await userRepository.Search(term);
        var tweeds = await tweedRepository.Search(term);
        ResultsViewModel viewModel = new(
            term,
            users.Select(u => new UserViewModel(u.Id!, u.UserName)).ToList(),
            tweeds.Select(t => new TweedViewModel(t.Id!, t.Text)).ToList()
        );

        if (Request.IsHtmx())
            return PartialView("Results", viewModel);
        return View("Results", viewModel);
    }
}
