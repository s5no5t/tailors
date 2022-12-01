using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");

        var tweedQueriesMock = new Mock<ITweedQueries>();
        var indexModel = new ProfilePageModel(tweedQueriesMock.Object, _userManagerMock.Object);

        await indexModel.OnGetAsync();

        _userManagerMock.Verify(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()));
        tweedQueriesMock.Verify(t => t.GetTweedsForUser("user1"));
    }
}
