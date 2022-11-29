using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Pages;
using Tweed.Web.Test.Helper;
using Xunit;

namespace Tweed.Web.Test.Pages;

public class LikeModelTest
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public LikeModelTest()
    {
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
    }

    [Fact]
    public async Task OnPost_ShouldIncreaseLikes()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var likeModel = new LikeModel(tweedQueriesMock.Object, _userManagerMock.Object)
        {
            Id = "123"
        };
        await likeModel.OnPostAsync();
        tweedQueriesMock.Verify(t => t.AddLike("123"));
    }

    [Fact]
    public void LikeModel_RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(LikeModel), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }

    [Fact]
    public async Task OnPostAsync_WhenIdIsNull_ReturnsBadRequest()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var createModel = new LikeModel(tweedQueriesMock.Object, _userManagerMock.Object);

        createModel.Validate();
        var result = await createModel.OnPostAsync();

        Assert.IsType<BadRequestResult>(result);
    }
}
