using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Helper;
using Tweed.Web.Pages;
using Tweed.Web.Test.TestHelper;
using Xunit;

namespace Tweed.Web.Test.Pages;

public class CreatePageModelTest
{
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Mock<ITweedQueries> _tweedQueriesMock;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public CreatePageModelTest()
    {
        _tweedQueriesMock = new Mock<ITweedQueries>();
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
        _notificationManagerMock = new Mock<INotificationManager>();
    }

    [Fact]
    public void CreateModel_RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(CreatePageModel), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task OnPostAsync_WhenTextIsNull_ReturnsPageResult()
    {
        var createModel = new CreatePageModel(_tweedQueriesMock.Object, _userManagerMock.Object,
            _notificationManagerMock.Object);

        createModel.Validate();
        var result = await createModel.OnPostAsync();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_WhenTextIsLongerThan280Chars_ReturnsPageResult()
    {
        var createModel = new CreatePageModel(_tweedQueriesMock.Object, _userManagerMock.Object,
            _notificationManagerMock.Object)
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
        var createModel = new CreatePageModel(_tweedQueriesMock.Object, _userManagerMock.Object,
            _notificationManagerMock.Object);

        var result = await createModel.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_SavesTweed()
    {
        var principal = PageModelTestHelper.BuildPrincipal();
        _userManagerMock.Setup(u => u.GetUserId(principal)).Returns("user1");
        var createModel = new CreatePageModel(_tweedQueriesMock.Object, _userManagerMock.Object,
            _notificationManagerMock.Object)
        {
            PageContext = PageModelTestHelper.BuildPageContext(principal),
            Text = "text"
        };

        await createModel.OnPostAsync();

        _tweedQueriesMock.Verify(t =>
            t.StoreTweed("text", "user1", It.IsAny<ZonedDateTime>()));
    }

    [Fact]
    public async Task OnPostAsync_ValidModel_SetsSuccessMessage()
    {
        var createModel = new CreatePageModel(_tweedQueriesMock.Object, _userManagerMock.Object,
            _notificationManagerMock.Object);

        await createModel.OnPostAsync();

        _notificationManagerMock.Verify(n => n.AppendSuccess("Tweed Posted"));
    }
}
