using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserFollowsAggregate;
using Tailors.Domain.UserLikesAggregate;
using Tailors.Web.Features.Profile;
using Tailors.Web.Helper;
using Tailors.Web.Test.TestHelper;
using Xunit;

namespace Tailors.Web.Test.Controllers;

public class ProfileControllerTest
{
    private readonly AppUser _currentUser = new()
    {
        Id = "currentUser"
    };

    private readonly FollowUserUseCase _followUserUseCase;

    private readonly AppUser _profileUser = new()
    {
        Id = "user"
    };

    private readonly ProfileController _sut;
    private readonly TweedRepositoryMock _tweedRepositoryMock = new();
    private readonly UserFollowsRepositoryMock _userFollowsRepositoryMock = new();

    public ProfileControllerTest()
    {
        var store = new UserStoreMock();
        store.Create(_currentUser);
        store.Create(_profileUser);
        var userManagerMock = UserManagerBuilder.CreateUserManager(store);
        var currentUserPrincipal = ControllerTestHelper.BuildPrincipal(_currentUser.Id!);
        _followUserUseCase = new FollowUserUseCase(_userFollowsRepositoryMock);
        var viewModelFactory = new TweedViewModelFactory(
            new UserLikesRepositoryMock(),
            new LikeTweedUseCase(new UserLikesRepositoryMock()),
            userManagerMock);

        _sut = new ProfileController(_tweedRepositoryMock, userManagerMock, viewModelFactory,
            _userFollowsRepositoryMock, _followUserUseCase)
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
        await _tweedRepositoryMock.Create(new Tweed("user", "text", DateTime.UtcNow));

        var result = await _sut.Index("user");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal("user", viewModel.Tweeds[0].AuthorId);
    }

    [Fact]
    public async Task Index_ShouldReturnNotFound_WhenUserIdDoesntExist()
    {
        var result = await _sut.Index("unknownUser");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Index_ShouldLoadUserName()
    {
        _profileUser.UserName = "UserName";

        var result = await _sut.Index("user");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal("UserName", viewModel.UserName);
    }

    [Fact]
    public async Task Index_ShouldSetCurrentUserFollowsIsTrue_WhenCurrentUserIsFollower()
    {
        UserFollows userFollows = new("currentUser");
        userFollows.AddFollows(_profileUser.Id!, DateTime.UtcNow);
        await _userFollowsRepositoryMock.Create(userFollows);

        var result = await _sut.Index("user");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.True(viewModel.CurrentUserFollows);
    }

    [Fact]
    public async Task Index_ShouldSetCurrentUserFollowsIsFalse_WhenCurrentUserIsNotFollower()
    {
        var result = await _sut.Index("user");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.False(viewModel.CurrentUserFollows);
    }

    [Fact]
    public async Task Index_ShouldSetUserId()
    {
        var result = await _sut.Index("user");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal("user", viewModel.UserId);
    }

    [Fact]
    public async Task Index_ShouldSetFollowersCount()
    {
        UserFollows userFollows = new("user");
        userFollows.AddFollows("otherUser", DateTime.UtcNow);
        await _userFollowsRepositoryMock.Create(userFollows);
        _profileUser.Id = "user";

        var result = await _sut.Index("user");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal(1, viewModel.FollowersCount);
    }

    [Fact]
    public async Task Follow_ShouldReturnNotFound_WhenUserIdNotFound()
    {
        var result = await _sut.Follow("unknownUser");
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Follow_ShouldReturnBadRequest_WhenTryingToFollowCurrentUser()
    {
        var result = await _sut.Follow("currentUser");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Follow_ShouldAddFollower()
    {
        UserFollows userFollows = new("currentUser");
        await _userFollowsRepositoryMock.Create(userFollows);

        await _sut.Follow("user");

        Assert.Contains(await _followUserUseCase.GetFollows("currentUser"), l => l.LeaderId == "user");
    }

    [Fact]
    public async Task Follow_ShouldRedirectToIndex()
    {
        var result = await _sut.Follow("user");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("user", ((RedirectToActionResult)result).RouteValues!["userId"]);
    }

    [Fact]
    public async Task Unfollow_ShouldReturnNotFound_WhenLeaderIdNotFound()
    {
        var result = await _sut.Unfollow("unknownUser");
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Unfollow_ShouldRemoveFollower()
    {
        UserFollows userFollows = new("currentUser");
        userFollows.AddFollows(_profileUser.Id!, DateTime.UtcNow);
        await _userFollowsRepositoryMock.Create(userFollows);

        await _sut.Unfollow("user");

        Assert.DoesNotContain(await _followUserUseCase.GetFollows("currentUser"), l => l.LeaderId == "user");
    }

    [Fact]
    public async Task Unfollow_ShouldRedirectToIndex()
    {
        var result = await _sut.Unfollow("user");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("user", ((RedirectToActionResult)result).RouteValues!["userId"]);
    }
}
