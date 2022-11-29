using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Tweed.Data;
using Tweed.Data.Entities;
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
    public void CreateModel_RequiresAuthorization()
    {
        var authorizeAttributeValue = Attribute.GetCustomAttribute(typeof(CreateModel), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task OnPostAsync_WhenTextIsNull_ReturnsPageResult()
    {
        var createModel = new CreateModel(_tweedQueriesMock.Object, _userManagerMock.Object);

        createModel.Validate();
        var result = await createModel.OnPostAsync();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_WhenTextIsLongerThan280Chars_ReturnsPageResult()
    {
        var createModel = new CreateModel(_tweedQueriesMock.Object, _userManagerMock.Object)
        {
            Text = new string('a', 281)
        };

        createModel.Validate();
        var result = await createModel.OnPostAsync();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_ValidModel_ReturnsRedirectToPageResult()
    {
        var createModel = new CreateModel(_tweedQueriesMock.Object, _userManagerMock.Object);
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
        await createModel.OnPostAsync();

        _tweedQueriesMock.Verify(t => t.StoreTweed(It.IsAny<Data.Entities.Tweed>()));
    }
}
