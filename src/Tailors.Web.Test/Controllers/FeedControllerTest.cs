using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Web.Test.TestHelper;
using Tailors.Web.Features.Feed;
using Tailors.Web.Features.Shared;
using Tailors.Web.Helper;
using Xunit;

namespace Tailors.Web.Test.Controllers;

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
        var tweed = new Tweed(authorId: "author", id: "twedId", createdAt: DateTime.Now, text: string.Empty);
        _showFeedUseCaseMock.Setup(t => t.GetFeed("currentUser", It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed> { tweed });
        _viewModelFactoryMock.Setup(v => v.Create(tweed, false))
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
        var tweed = new Tweed(authorId: "author", id: "tweedId", createdAt: DateTime.Now, text: string.Empty);
        _showFeedUseCaseMock.Setup(t => t.GetFeed("currentUser", 0, It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed> { tweed });
        _viewModelFactoryMock
            .Setup(v => v.Create(It.IsAny<List<Tweed>>(), "none"))
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
        var tweed = new Tweed(authorId: "author", id: "tweedId", createdAt: DateTime.Now, text: string.Empty);
        _showFeedUseCaseMock.Setup(t => t.GetFeed("currentUser", 0, It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed> { tweed });
        _viewModelFactoryMock
            .Setup(v => v.Create(It.IsAny<List<Tweed>>(), It.IsAny<string>()))
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

    [Fact]
    public async Task UpdateAvailable_ShouldReturnTrue_WhenThereIsANewTweed()
    {
        var instant = new DateTime(2023, 5, 22, 10, 0, 0);
        var tweed = new Tweed(id: "tweedId", createdAt: instant.AddMinutes(5), authorId: "authorId", text: string.Empty);
        _showFeedUseCaseMock.Setup(t => t.GetFeed("currentUser", 0, It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed> { tweed });
        _feedController.ControllerContext.HttpContext.Request.Headers["Hx-Request"] = "true";

        var result = await _feedController.NewTweedsNotification(instant);

        var resultViewModel = (NewTweedsNotificationViewModel)((PartialViewResult)result).Model!;
        Assert.True(resultViewModel.NewTweedsAvailable);
    }

    [Fact]
    public async Task UpdateAvailable_ShouldReturnFalse_WhenThereIsNoNewTweed()
    {
        var instant = new DateTime(2023, 5, 22, 10, 0, 0);
        var tweed = new Tweed(id: "tweedId", createdAt: instant.AddMinutes(-5), authorId: "authorId", text: string.Empty);
        _showFeedUseCaseMock.Setup(t => t.GetFeed("currentUser", 0, It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed> { tweed });
        _feedController.ControllerContext.HttpContext.Request.Headers["Hx-Request"] = "true";

        var result = await _feedController.NewTweedsNotification(instant);

        var resultViewModel = (NewTweedsNotificationViewModel)((PartialViewResult)result).Model!;
        Assert.False(resultViewModel.NewTweedsAvailable);
    }

    [Fact]
    public async Task UpdateAvailable_ShouldReturnRedirect_WhenRequestIsNotFromHtmx()
    {
        var instant = new DateTime(2023, 5, 22, 10, 0, 0);

        await Assert.ThrowsAsync<Exception>(async () =>
            await _feedController.NewTweedsNotification(instant));
    }
}
