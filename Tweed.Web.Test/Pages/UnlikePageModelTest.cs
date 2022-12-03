using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Pages;
using Tweed.Web.Test.TestHelper;
using Xunit;

namespace Tweed.Web.Test.Pages;

public class UnlikePageModelTest
{
    private readonly HeaderDictionary _headers;
    private readonly PageContext _pageContext;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public UnlikePageModelTest()
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
    public async Task OnPost_ShouldDecreaseLikes()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
        var unlikePageModel = new UnlikePageModel(tweedQueriesMock.Object, _userManagerMock.Object)
        {
            Id = "123",
            PageContext = _pageContext
        };

        await unlikePageModel.OnPostAsync();

        tweedQueriesMock.Verify(t => t.RemoveLike("123", "user1"));
    }

    [Fact]
    public void LikeModel_RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(UnlikePageModel), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task OnPostAsync_WhenIdIsNull_ReturnsBadRequest()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var unlikePageModel = new UnlikePageModel(tweedQueriesMock.Object, _userManagerMock.Object);

        unlikePageModel.Validate();
        var result = await unlikePageModel.OnPostAsync();

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnRedirect()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var unlikePageModel = new UnlikePageModel(tweedQueriesMock.Object, _userManagerMock.Object)
        {
            PageContext = _pageContext
        };
        _headers.Append("Referer", "https://example.com");

        var result = await unlikePageModel.OnPostAsync();

        Assert.IsType<RedirectResult>(result);
    }
}
