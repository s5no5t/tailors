using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Pages;
using Tweed.Web.Test.TestHelper;
using Xunit;

namespace Tweed.Web.Test.Pages;

public class LikePageModelTest
{
    private readonly HeaderDictionary _headers;
    private readonly PageContext _pageContext;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public LikePageModelTest()
    {
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();

        _pageContext = new PageContext();
        var httpContext = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        _headers = new HeaderDictionary();
        request.Setup(r => r.Headers).Returns(_headers);
        httpContext.Setup(h => h.Request).Returns(request.Object);
        _pageContext.HttpContext = httpContext.Object;
    }

    [Fact]
    public async Task OnPost_ShouldIncreaseLikes()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
        var likeModel = new LikePageModel(tweedQueriesMock.Object, _userManagerMock.Object)
        {
            Id = "123",
            PageContext = _pageContext
        };

        await likeModel.OnPostAsync();

        tweedQueriesMock.Verify(t => t.AddLike("123", "user1", It.IsAny<ZonedDateTime>()));
    }

    [Fact]
    public void LikeModel_RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(LikePageModel), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task OnPostAsync_WhenIdIsNull_ReturnsBadRequest()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var createModel = new LikePageModel(tweedQueriesMock.Object, _userManagerMock.Object);

        createModel.Validate();
        var result = await createModel.OnPostAsync();

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnRedirect()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var likePageModel = new LikePageModel(tweedQueriesMock.Object, _userManagerMock.Object)
        {
            PageContext = _pageContext
        };
        _headers.Append("Referer", "https://example.com");

        var result = await likePageModel.OnPostAsync();

        Assert.IsType<RedirectResult>(result);
    }
}
