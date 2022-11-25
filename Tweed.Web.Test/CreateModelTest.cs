using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Tweed.Data;
using Tweed.Web.Pages;
using Xunit;

namespace Tweed.Web.Test;

public class CreateModelTest
{
    private readonly Mock<ITweedQueries> _tweedQueriesMock;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public CreateModelTest()
    {
        _tweedQueriesMock = new Mock<ITweedQueries>();
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
    }

    [Fact]
    public async Task OnPostAsync_InvalidModel_ReturnsPageResult()
    {
        var createModel = new CreateModel(_tweedQueriesMock.Object, _userManagerMock.Object);
        createModel.ModelState.AddModelError("someKey", "errorMessage");
        var result = await createModel.OnPostAsync();
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_ValidModel_ReturnsRedirectToPageResult()
    {
        var createModel = new CreateModel(_tweedQueriesMock.Object, _userManagerMock.Object)
        {
            Tweed = new Data.Models.Tweed()
        };
        var result = await createModel.OnPostAsync();
        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_SavesTweed()
    {
        var principal = PageModelTestHelper.BuildPrincipal();
        _userManagerMock.Setup(u => u.GetUserId(principal)).Returns("123");
        var createModel = new CreateModel(_tweedQueriesMock.Object, _userManagerMock.Object)
        {
            PageContext = PageModelTestHelper.BuildPageContext(principal)
        };
        var tweed = new Data.Models.Tweed();
        createModel.Tweed = tweed;
        await createModel.OnPostAsync();

        _tweedQueriesMock.Verify(t => t.CreateTweed(tweed, "123"));
    }
}
