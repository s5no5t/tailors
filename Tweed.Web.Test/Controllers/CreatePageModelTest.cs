using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NodaTime;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Controllers;
using Tweed.Web.Helper;
using Tweed.Web.Test.TestHelper;
using Tweed.Web.Views.Tweed;
using Xunit;

namespace Tweed.Web.Test.Controllers;

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
    public void RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(TweedController), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task OnPostAsync_ValidModel_ReturnsRedirectToPageResult()
    {
        var createModel = new TweedController(_tweedQueriesMock.Object, _userManagerMock.Object,
            _notificationManagerMock.Object);

        CreateViewModel viewModel = new()
        {
            Text = "test"
        };
        var result = await createModel.Create(viewModel);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_SavesTweed()
    {
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
        var createModel = new TweedController(_tweedQueriesMock.Object, _userManagerMock.Object,
            _notificationManagerMock.Object);

        CreateViewModel viewModel = new()
        {
            Text = "text"
        };
        await createModel.Create(viewModel);

        _tweedQueriesMock.Verify(t =>
            t.StoreTweed("text", "user1", It.IsAny<ZonedDateTime>()));
    }

    [Fact]
    public async Task OnPostAsync_ValidModel_SetsSuccessMessage()
    {
        var createModel = new TweedController(_tweedQueriesMock.Object, _userManagerMock.Object,
            _notificationManagerMock.Object);

        CreateViewModel viewModel = new()
        {
            Text = "test"
        };
        await createModel.Create(viewModel);

        _notificationManagerMock.Verify(n => n.AppendSuccess("Tweed Posted"));
    }
}
