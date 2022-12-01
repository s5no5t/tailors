using System;
using Microsoft.AspNetCore.Authorization;
using Tweed.Web.Pages;
using Xunit;

namespace Tweed.Web.Test.Pages;

public class ProfileModelTest
{
    [Fact]
    public void CreateModel_RequiresAuthorization()
    {
        var authorizeAttributeValue =
            Attribute.GetCustomAttribute(typeof(ProfilePageModel), typeof(AuthorizeAttribute));
        Assert.NotNull(authorizeAttributeValue);
    }
}
