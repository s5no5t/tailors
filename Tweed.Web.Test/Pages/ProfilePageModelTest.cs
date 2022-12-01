using System;
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
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var indexModel = new ProfilePageModel(tweedQueriesMock.Object, _userManagerMock.Object);

        await indexModel.OnGetAsync();

        tweedQueriesMock.Verify(t => t.GetTweedsForUser("user1"));
    }
}
