using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Pages;
using Tweed.Web.Test.TestHelper;
using Xunit;

namespace Tweed.Web.Test.Pages;

public class ProfilePageModelTest
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public ProfilePageModelTest()
    {
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
    }

    [Fact]
    public void CreateModel_RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(ProfilePageModel), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task OnGet_ShouldLoadTweeds()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var appUser = new AppUser();
        _userManagerMock.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync(appUser);
        var indexModel = new ProfilePageModel(tweedQueriesMock.Object, _userManagerMock.Object)
        {
            UserId = "user1"
        };

        await indexModel.OnGetAsync();

        tweedQueriesMock.Verify(t => t.GetTweedsForUser("user1"));
    }

    [Fact]
    public async Task OnGet_ShouldReturnNotFound_WhenUserIdIsNull()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var indexModel = new ProfilePageModel(tweedQueriesMock.Object, _userManagerMock.Object)
        {
            UserId = null
        };

        var result = await indexModel.OnGetAsync();

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task OnGet_ShouldReturnNotFound_WhenUserIdDoesntExist()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        _userManagerMock.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync((AppUser)null!);
        var indexModel = new ProfilePageModel(tweedQueriesMock.Object, _userManagerMock.Object)
        {
            UserId = "user1"
        };

        var result = await indexModel.OnGetAsync();

        Assert.IsType<NotFoundResult>(result);
    }
}
