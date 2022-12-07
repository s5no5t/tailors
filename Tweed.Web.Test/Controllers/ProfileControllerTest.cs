using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Controllers;
using Tweed.Web.Test.TestHelper;
using Tweed.Web.Views.Profile;
using Xunit;

namespace Tweed.Web.Test.Controllers;

public class ProfileControllerTest
{
    private readonly Mock<IAppUserQueries> _appUserQueriesMock;

    private readonly AppUser _currentUser = new()
    {
        Id = "currentUser"
    };

    private readonly ProfileController _profileController;
    private readonly Mock<ITweedQueries> _tweedQueriesMock;
    private readonly AppUser _user;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public ProfileControllerTest()
    {
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
        var currentUserPrincipal = ControllerTestHelper.BuildPrincipal();
        _userManagerMock.Setup(u =>
            u.GetUserAsync(currentUserPrincipal)).ReturnsAsync(_currentUser);
        _userManagerMock.Setup(u =>
            u.GetUserId(currentUserPrincipal)).Returns(_currentUser.Id!);
        _user = new AppUser();
        _userManagerMock.Setup(u => u.FindByIdAsync("user")).ReturnsAsync(_user);
        _appUserQueriesMock = new Mock<IAppUserQueries>();
        _tweedQueriesMock = new Mock<ITweedQueries>();
        _tweedQueriesMock.Setup(t => t.GetTweedsForUser("user"))
            .ReturnsAsync(new List<Data.Entities.Tweed>());
        _profileController = new ProfileController(_tweedQueriesMock.Object,
            _userManagerMock.Object,
            _appUserQueriesMock.Object)
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

        _tweedQueriesMock.Verify(t => t.GetTweedsForUser("user"));
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
        _user.UserName = "UserName";

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
        _user.Id = "user";
        _currentUser.Follows.Add(new Follows
        {
            LeaderId = _user.Id
        });

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
        _user.Id = "user";

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
        _user.Id = "user";

        var result = await _profileController.Index("user");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal("user", viewModel.UserId);
    }

    [Fact]
    public async Task Follow_ShouldReturnNotFound_WhenLeaderIdNotFound()
    {
        var result = await _profileController.Follow("unknownUser");
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Follow_ShouldAddFollower()
    {
        await _profileController.Follow("user");

        _appUserQueriesMock.Verify(t =>
            t.AddFollower("user", "currentUser", It.IsAny<ZonedDateTime>()));
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

        _appUserQueriesMock.Verify(t =>
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
