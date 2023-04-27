﻿using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tweed.Data;
using Tweed.Data.Model;
using Tweed.Web.Helper;
using Tweed.Web.Views.Home;
using Tweed.Web.Views.Shared;

namespace Tweed.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IFeedBuilder _feedBuilder;
    private readonly UserManager<AppUser> _userManager;
    private readonly IViewModelFactory _viewModelFactory;

    public HomeController(IFeedBuilder feedBuilder, UserManager<AppUser> userManager,
        IViewModelFactory viewModelFactory)
    {
        _feedBuilder = feedBuilder;
        _userManager = userManager;
        _viewModelFactory = viewModelFactory;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User)!;
        var feed = await _feedBuilder.GetFeed(currentUser.Id!);

        List<TweedViewModel> tweedViewModels = new();
        foreach (var tweed in feed)
        {
            var tweedViewModel = await _viewModelFactory.BuildTweedViewModel(tweed);
            tweedViewModels.Add(tweedViewModel);
        }

        var viewModel = new IndexViewModel
        {
            Feed = new FeedViewModel
            {
                Tweeds = tweedViewModels
            }
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Feed(int page = 0)
    {
        // TODO
        var viewModel = new FeedViewModel
        {
            Page = page,
            Tweeds = new List<TweedViewModel>
            {
                new()
                {
                    Id = "123",
                    AuthorId = "123",
                    Author = "test",
                    Text = "test",
                    CreatedAt = "today",
                    LikesCount = 2,
                    LikedByCurrentUser = false
                }
            }
        };
        return PartialView("_Feed", viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
            { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
