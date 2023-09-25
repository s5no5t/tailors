using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tailors.Thread.Domain.TweedAggregate;
using Tailors.User.Domain;
using Tailors.Web.Test.TestHelper;
using Tailors.Web.Features.Profile;
using Tailors.Web.Helper;
using Xunit;

namespace Tailors.Web.Test.Controllers;

public class ProfileControllerTest
{
    private readonly AppUser _currentUser = new()
    {
        Id = "currentUser"
    };

    private readonly Mock<IFollowUserUseCase> _followUserUseCaseMock = new();

    private readonly ProfileController _profileController;

    private readonly AppUser _profileUser = new()
    {
        Id = "user"
    };

    private readonly Mock<ITweedRepository> _tweedRepositoryMock;
    private readonly Mock<IUserFollowsRepository> _userFollowsRepositoryMock = new();
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<ITweedViewModelFactory> _viewModelFactoryMock = new();

    public ProfileControllerTest()
    {
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
        var currentUserPrincipal = ControllerTestHelper.BuildPrincipal();
        _userManagerMock.Setup(u =>
            u.GetUserId(currentUserPrincipal)).Returns(_currentUser.Id!);
        _userManagerMock.Setup(u => u.FindByIdAsync("user")).ReturnsAsync(_profileUser);

        _userFollowsRepositoryMock.Setup(u => u.GetFollowerCount(It.IsAny<string>()))
            .ReturnsAsync(0);
        _followUserUseCaseMock.Setup(u => u.GetFollows(It.IsAny<string>()))
            .ReturnsAsync(new List<UserFollows.LeaderReference>());

        _tweedRepositoryMock = new Mock<ITweedRepository>();
        _tweedRepositoryMock.Setup(t => t.GetAllByAuthorId("user", It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed>());

        _profileController = new ProfileController(_tweedRepositoryMock.Object,
            _userManagerMock.Object, _viewModelFactoryMock.Object,
            _userFollowsRepositoryMock.Object, _followUserUseCaseMock.Object)
        {
            ControllerContext = ControllerTestHelper.BuildControllerContext(currentUserPrincipal)
        };
    }

    [Fact]
    public void RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(ProfileController), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task Index_ShouldLoadTweeds()
    {
        await _profileController.Index("user");

        _tweedRepositoryMock.Verify(t => t.GetAllByAuthorId("user", It.IsAny<int>()));
    }

    [Fact]
    public async Task Index_ShouldReturnNotFound_WhenUserIdDoesntExist()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync("unknownUser")).ReturnsAsync((AppUser)null!);

        var result = await _profileController.Index("unknownUser");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Index_ShouldLoadUserName()
    {
        _profileUser.UserName = "UserName";

        var result = await _profileController.Index("user");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal("UserName", viewModel.UserName);
    }

    [Fact]
    public async Task Index_ShouldSetCurrentUserFollowsIsTrue_WhenCurrentUserIsFollower()
    {
        var follows = new List<UserFollows.LeaderReference>
        {
            new()
            {
                LeaderId = _profileUser.Id
            }
        };
        _followUserUseCaseMock.Setup(f => f.GetFollows(_currentUser.Id!)).ReturnsAsync(follows);

        var result = await _profileController.Index("user");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.True(viewModel.CurrentUserFollows);
    }

    [Fact]
    public async Task Index_ShouldSetCurrentUserFollowsIsFalse_WhenCurrentUserIsNotFollower()
    {
        _followUserUseCaseMock.Setup(f => f.GetFollows(_currentUser.Id!))
            .ReturnsAsync(new List<UserFollows.LeaderReference>());

        var result = await _profileController.Index("user");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.False(viewModel.CurrentUserFollows);
    }

    [Fact]
    public async Task Index_ShouldSetUserId()
    {
        var result = await _profileController.Index("user");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal("user", viewModel.UserId);
    }

    [Fact]
    public async Task Index_ShouldSetFollowersCount()
    {
        _userFollowsRepositoryMock.Setup(u => u.GetFollowerCount("user")).ReturnsAsync(10);
        _profileUser.Id = "user";

        var result = await _profileController.Index("user");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal(10, viewModel.FollowersCount);

        _userFollowsRepositoryMock.Verify(u => u.GetFollowerCount("user"));
    }

    [Fact]
    public async Task Follow_ShouldReturnNotFound_WhenUserIdNotFound()
    {
        var result = await _profileController.Follow("unknownUser");
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Follow_ShouldReturnBadRequest_WhenTryingToFollowCurrentUser()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync("currentUser"))
            .ReturnsAsync(_currentUser);

        var result = await _profileController.Follow("currentUser");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Follow_ShouldAddFollower()
    {
        await _profileController.Follow("user");

        _followUserUseCaseMock.Verify(t =>
            t.AddFollower("user", "currentUser", It.IsAny<DateTime>()));
    }

    [Fact]
    public async Task Follow_ShouldRedirectToIndex()
    {
        var result = await _profileController.Follow("user");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("user", ((RedirectToActionResult)result).RouteValues!["userId"]);
    }

    [Fact]
    public async Task Unfollow_ShouldReturnNotFound_WhenLeaderIdNotFound()
    {
        var result = await _profileController.Unfollow("unknownUser");
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Unfollow_ShouldAddFollower()
    {
        await _profileController.Unfollow("user");

        _followUserUseCaseMock.Verify(t =>
            t.RemoveFollower("user", "currentUser"));
    }

    [Fact]
    public async Task Unfollow_ShouldRedirectToIndex()
    {
        var result = await _profileController.Unfollow("user");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("user", ((RedirectToActionResult)result).RouteValues!["userId"]);
    }
}
