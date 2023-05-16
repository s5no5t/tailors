using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tweed.Feed.Domain;
using Tweed.Thread.Domain;
using Tweed.User.Domain;
using Tweed.Web.Controllers;
using Tweed.Web.Helper;
using Tweed.Web.Test.TestHelper;
using Tweed.Web.Views.Feed;
using Tweed.Web.Views.Shared;
using Xunit;

namespace Tweed.Web.Test.Controllers;

public class FeedControllerTest
{
    private readonly ClaimsPrincipal _currentUserPrincipal = ControllerTestHelper.BuildPrincipal();

    private readonly FeedController _feedController;
    private readonly Mock<IShowFeedUseCase> _showFeedUseCaseMock = new();

    private readonly Mock<UserManager<AppUser>> _userManagerMock =
        UserManagerMockHelper.MockUserManager<AppUser>();

    private readonly Mock<ITweedViewModelFactory> _viewModelFactoryMock = new();

    public FeedControllerTest()
    {
        var user = new AppUser
        {
            Id = "currentUser"
        };
        _userManagerMock.Setup(u => u.GetUserId(_currentUserPrincipal)).Returns(user.Id);
        var tweed = new Thread.Domain.Tweed
        {
            Id = "tweedId",
            AuthorId = "author"
        };
        _showFeedUseCaseMock.Setup(t => t.GetFeed("currentUser", It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Thread.Domain.Tweed> { tweed });
        _viewModelFactoryMock.Setup(v => v.Create(tweed))
            .ReturnsAsync(new TweedViewModel());

        _feedController = new FeedController(_showFeedUseCaseMock.Object, _userManagerMock.Object,
            _viewModelFactoryMock.Object)
        {
            ControllerContext = ControllerTestHelper.BuildControllerContext(_currentUserPrincipal)
        };
    }

    [Fact]
    public void RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(FeedController), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task Index_ShouldReturnIndexViewModel()
    {
        var result = await _feedController.Index();

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
    }

    [Fact]
    public async Task Index_ShouldReturnPage0()
    {
        var result = await _feedController.Index();

        var model = ((result as ViewResult)!.Model as IndexViewModel)!;
        Assert.Equal(0, model.Feed.Page);
    }

    [Fact]
    public async Task Index_ShouldReturnTweeds()
    {
        var tweed = new Thread.Domain.Tweed
        {
            Id = "tweedId",
            AuthorId = "author"
        };
        _showFeedUseCaseMock.Setup(t => t.GetFeed("currentUser", 0, It.IsAny<int>()))
            .ReturnsAsync(new List<Thread.Domain.Tweed> { tweed });
        _viewModelFactoryMock
            .Setup(v => v.Create(It.IsAny<List<Thread.Domain.Tweed>>()))
            .ReturnsAsync(new List<TweedViewModel>
            {
                new()
                {
                    Id = tweed.Id
                }
            });

        var result = await _feedController.Index();

        var model = ((result as ViewResult)!.Model as IndexViewModel)!;
        Assert.Equal(tweed.Id, model.Feed.Tweeds[0].Id);
    }

    [Fact]
    public async Task Feed_ShouldReturnFeedPartialViewModel()
    {
        var result = await _feedController.Feed();

        Assert.IsType<PartialViewResult>(result);
        var resultAsView = (PartialViewResult)result;
        Assert.IsType<FeedViewModel>(resultAsView.Model);
    }

    [Fact]
    public async Task Feed_ShouldReturnPage1_WhenPageIs1()
    {
        var result = await _feedController.Feed(1);

        var model = ((result as PartialViewResult)!.Model as FeedViewModel)!;

        Assert.Equal(1, model.Page);
    }

    [Fact]
    public async Task Feed_ShouldReturnTweeds()
    {
        var tweed = new Thread.Domain.Tweed
        {
            Id = "tweedId",
            AuthorId = "author"
        };
        _showFeedUseCaseMock.Setup(t => t.GetFeed("currentUser", 0, It.IsAny<int>()))
            .ReturnsAsync(new List<Thread.Domain.Tweed> { tweed });
        _viewModelFactoryMock
            .Setup(v => v.Create(It.IsAny<List<Thread.Domain.Tweed>>()))
            .ReturnsAsync(new List<TweedViewModel>
            {
                new()
                {
                    Id = tweed.Id
                }
            });

        var result = await _feedController.Feed();

        var model = ((result as PartialViewResult)!.Model as FeedViewModel)!;
        Assert.Equal(tweed.Id, model.Tweeds[0].Id);
    }
}