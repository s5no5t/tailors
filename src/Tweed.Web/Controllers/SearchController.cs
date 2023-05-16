using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tweed.Thread.Domain;
using Tweed.User.Domain;
using Tweed.Web.Views.Search;

namespace Tweed.Web.Controllers;

[Authorize]
public class SearchController : Controller
{
    public IActionResult Index()
    {
        IndexViewModel viewModel = new("", new List<UserViewModel>(), new List<TweedViewModel>());
        return View(viewModel);
    }

    public async Task<IActionResult> Results(
        [FromQuery] [Required] [StringLength(50, MinimumLength = 3)] [RegularExpression(@"^[\w\s]*$")]
        string term,
        [FromServices] ITweedRepository tweedRepository,
        [FromServices] IUserRepository userRepository)
    {
        if (!ModelState.IsValid)
            return View("Index",
                new IndexViewModel(term, new List<UserViewModel>(), new List<TweedViewModel>()));

        var users = await userRepository.Search(term);
        var tweeds = await tweedRepository.Search(term);
        IndexViewModel viewModel = new(
            term,
            users.Select(u => new UserViewModel(u.Id!, u.UserName!)).ToList(),
            tweeds.Select(t => new TweedViewModel(t.Id!, t.Text!)).ToList()
        );
        return View("index", viewModel);
    }
}