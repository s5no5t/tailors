using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tweed.Domain;
using Tweed.Domain.Model;
using Tweed.Feed.Domain;
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
    private readonly Mock<IShowFeedUseCase> _showFeedUseCaseMock = new();
    private readonly Mock<ITweedViewModelFactory> _viewModelFactoryMock = new();

    private readonly Mock<UserManager<User>> _userManagerMock =
        UserManagerMockHelper.MockUserManager<User>();

    private readonly FeedController _feedController;

    public FeedControllerTest()
    {
        var user = new User
        {
            Id = "currentUser"
        };
        _userManagerMock.Setup(u => u.GetUserId(_currentUserPrincipal)).Returns(user.Id);
        var tweed = new Domain.Model.Tweed
        {
            Id = "tweedId",
            AuthorId = "author"
        };
        _showFeedUseCaseMock.Setup(t => t.GetFeed("currentUser", It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Tweed> { tweed });
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
        var tweed = new Domain.Model.Tweed
        {
            Id = "tweedId",
            AuthorId = "author"
        };
        _showFeedUseCaseMock.Setup(t => t.GetFeed("currentUser", 0, It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Tweed> { tweed });
        _viewModelFactoryMock
            .Setup(v => v.Create(It.IsAny<List<Domain.Model.Tweed>>()))
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
        var tweed = new Domain.Model.Tweed
        {
            Id = "tweedId",
            AuthorId = "author"
        };
        _showFeedUseCaseMock.Setup(t => t.GetFeed("currentUser", 0, It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Tweed> { tweed });
        _viewModelFactoryMock
            .Setup(v => v.Create(It.IsAny<List<Domain.Model.Tweed>>()))
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
