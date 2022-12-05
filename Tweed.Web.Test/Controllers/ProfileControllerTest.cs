using System;
using System.Collections.Generic;
using System.Security.Claims;
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
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public ProfileControllerTest()
    {
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
        _appUserQueriesMock = new Mock<IAppUserQueries>();
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
        var tweedQueriesMock = new Mock<ITweedQueries>();
        tweedQueriesMock.Setup(t => t.GetTweedsForUser("user1"))
            .ReturnsAsync(new List<Data.Entities.Tweed>());
        var appUser = new AppUser();
        _userManagerMock.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync(appUser);
        var controller = new ProfileController(tweedQueriesMock.Object, _userManagerMock.Object,
            _appUserQueriesMock.Object);

        await controller.Index("user1");

        tweedQueriesMock.Verify(t => t.GetTweedsForUser("user1"));
    }

    [Fact]
    public async Task Index_ShouldReturnNotFound_WhenUserIdDoesntExist()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        _userManagerMock.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync((AppUser)null!);
        var controller = new ProfileController(tweedQueriesMock.Object, _userManagerMock.Object,
            _appUserQueriesMock.Object);

        var result = await controller.Index("user1");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Index_ShouldLoadUserName()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        tweedQueriesMock.Setup(t => t.GetTweedsForUser("user1"))
            .ReturnsAsync(new List<Data.Entities.Tweed>());
        var appUser = new AppUser
        {
            UserName = "User 1"
        };
        _userManagerMock.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync(appUser);
        var controller = new ProfileController(tweedQueriesMock.Object, _userManagerMock.Object,
            _appUserQueriesMock.Object);

        var result = await controller.Index("user1");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal("User 1", viewModel.UserName);
    }


    [Fact]
    public async Task Follow_ShouldReturnNotFound_WhenLeaderIdNotFound()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var controller = new ProfileController(tweedQueriesMock.Object, _userManagerMock.Object,
            _appUserQueriesMock.Object);

        var result = await controller.Follow("user2");
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Follow_ShouldAddFollower()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var appUser = new AppUser
        {
            UserName = "User 2"
        };
        _userManagerMock.Setup(u => u.FindByIdAsync("user2")).ReturnsAsync(appUser);
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
        var controller = new ProfileController(tweedQueriesMock.Object, _userManagerMock.Object,
            _appUserQueriesMock.Object);

        await controller.Follow("user2");

        _appUserQueriesMock.Verify(t => t.AddFollower("user2", "user1", It.IsAny<ZonedDateTime>()));
    }
}
