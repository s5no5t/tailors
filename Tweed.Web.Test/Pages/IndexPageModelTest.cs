using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Moq;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Pages;
using Tweed.Web.Test.TestHelper;
using Xunit;

namespace Tweed.Web.Test.Pages;

public class IndexPageModelTest
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public IndexPageModelTest()
    {
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
    }

    [Fact]
    public void IndexPageModel_RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(IndexPageModel), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task OnGet_ShouldLoadLatestTweeds()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var indexModel = new IndexPageModel(tweedQueriesMock.Object, _userManagerMock.Object);

        await indexModel.OnGetAsync();

        tweedQueriesMock.Verify(t => t.GetLatestTweeds());
    }

    [Fact]
    public async Task OnGet_ShouldMarkTweedsWrittenByCurrentUser()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();

        var fixedZonedDateTime = new ZonedDateTime(new LocalDateTime(2022, 11, 18, 15, 20),
            DateTimeZone.Utc, new Offset());
        var tweed = new Data.Entities.Tweed
        {
            LikedBy = new List<LikedBy>
                { new() { UserId = "user1", LikedAt = fixedZonedDateTime } },
            AuthorId = "user2"
        };
        tweedQueriesMock.Setup(t => t.GetLatestTweeds()).ReturnsAsync(new[] { tweed });
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
        var appUser = new AppUser
        {
            UserName = "User 2"
        };
        _userManagerMock.Setup(u => u.FindByIdAsync("user2")).ReturnsAsync(appUser);
        var indexModel = new IndexPageModel(tweedQueriesMock.Object, _userManagerMock.Object);

        await indexModel.OnGetAsync();

        Assert.True(indexModel.Tweeds[0].LikedByCurrentUser);
    }
}
