using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
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
    private readonly Mock<ITweedRepository> _tweedRepositoryMock;
    private readonly Mock<IUserFollowsRepository> _userFollowsRepositoryMock = new();
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public ProfileControllerTest()
    {
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
        var currentUserPrincipal = ControllerTestHelper.BuildPrincipal();
        _userManagerMock.Setup(u =>
            u.GetUserId(currentUserPrincipal)).Returns(_currentUser.Id!);
        _userManagerMock.Setup(u => u.FindByIdAsync("user")).ReturnsAsync(_profileUser);
        _userFollowsRepositoryMock.Setup(u => u.GetById(It.IsAny<string>()))
            .ReturnsAsync(new None());
        _userFollowsRepositoryMock.Setup(u => u.GetFollowerCount(It.IsAny<string>()))
            .ReturnsAsync(0);
        _followUserUseCase = new FollowUserUseCase(_userFollowsRepositoryMock.Object);
        _tweedRepositoryMock = new Mock<ITweedRepository>();
        _tweedRepositoryMock.Setup(t => t.GetAllByAuthorId("user", It.IsAny<int>()))
            .ReturnsAsync(new List<Tweed>());
        var viewModelFactory = new TweedViewModelFactory(
            new UserLikesRepositoryMock(),
            new LikeTweedUseCase(new UserLikesRepositoryMock()),
            _userManagerMock.Object);

        _sut = new ProfileController(_tweedRepositoryMock.Object,
            _userManagerMock.Object, viewModelFactory, _userFollowsRepositoryMock.Object, _followUserUseCase)
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
        await _sut.Index("user");

        _tweedRepositoryMock.Verify(t => t.GetAllByAuthorId("user", It.IsAny<int>()));
    }

    [Fact]
    public async Task Index_ShouldReturnNotFound_WhenUserIdDoesntExist()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync("unknownUser")).ReturnsAsync((AppUser)null!);

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
        _userFollowsRepositoryMock.Setup(u => u.GetById("currentUser/Follows"))
            .ReturnsAsync(userFollows);

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
        _userFollowsRepositoryMock.Setup(u => u.GetFollowerCount("user")).ReturnsAsync(10);
        _profileUser.Id = "user";

        var result = await _sut.Index("user");

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
        var result = await _sut.Follow("unknownUser");
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Follow_ShouldReturnBadRequest_WhenTryingToFollowCurrentUser()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync("currentUser"))
            .ReturnsAsync(_currentUser);

        var result = await _sut.Follow("currentUser");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Follow_ShouldAddFollower()
    {
        UserFollows userFollows = new("currentUser");
        _userFollowsRepositoryMock.Setup(u => u.GetById("currentUser/Follows"))
            .ReturnsAsync(userFollows);

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
        _userFollowsRepositoryMock.Setup(u => u.GetById("currentUser/Follows"))
            .ReturnsAsync(userFollows);

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
