using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tweed.Web.Controllers;

[Authorize]
public class SearchController : Controller
{
    public async Task<IActionResult> Index()
    {
        return View();
    }
}


