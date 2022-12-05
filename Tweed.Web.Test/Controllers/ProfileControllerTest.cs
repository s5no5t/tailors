using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Controllers;
using Tweed.Web.Test.TestHelper;
using Tweed.Web.Views.Profile;
using Xunit;

namespace Tweed.Web.Test.Controllers;

public class ProfileControllerTest
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public ProfileControllerTest()
    {
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
    }

    [Fact]
    public void CreateModel_RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(ProfileController), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task OnGet_ShouldLoadTweeds()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        tweedQueriesMock.Setup(t => t.GetTweedsForUser("user1"))
            .ReturnsAsync(new List<Data.Entities.Tweed>());
        var appUser = new AppUser();
        _userManagerMock.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync(appUser);
        var controller = new ProfileController(tweedQueriesMock.Object, _userManagerMock.Object);

        await controller.Index("user1");

        tweedQueriesMock.Verify(t => t.GetTweedsForUser("user1"));
    }

    [Fact]
    public async Task OnGet_ShouldReturnNotFound_WhenUserIdDoesntExist()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        _userManagerMock.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync((AppUser)null!);
        var controller = new ProfileController(tweedQueriesMock.Object, _userManagerMock.Object);

        var result = await controller.Index("user1");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task OnGet_ShouldLoadUserName()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        tweedQueriesMock.Setup(t => t.GetTweedsForUser("user1"))
            .ReturnsAsync(new List<Data.Entities.Tweed>());
        var appUser = new AppUser
        {
            UserName = "User 1"
        };
        _userManagerMock.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync(appUser);
        var controller = new ProfileController(tweedQueriesMock.Object, _userManagerMock.Object);

        var result = await controller.Index("user1");

        Assert.IsType<ViewResult>(result);
        var resultAsView = (ViewResult)result;
        Assert.IsType<IndexViewModel>(resultAsView.Model);
        var viewModel = (IndexViewModel)resultAsView.Model!;
        Assert.Equal("User 1", viewModel.UserName);
    }
}
