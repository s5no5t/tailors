using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tailors.Web.Test.TestHelper;

internal static class ControllerTestHelper
{
    internal static ClaimsPrincipal BuildPrincipal(string userId)
    {
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        return principal;
    }

    internal static ControllerContext BuildControllerContext(ClaimsPrincipal user)
    {
        // var urlHelperMock = new Mock<IUrlHelper>();
        // var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        // urlHelperFactoryMock.Setup(u => u.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelperMock.Object);
        // var serviceProviderMock = new Mock<IServiceProvider>();
        // serviceProviderMock.Setup(sp => sp.GetService(typeof(IUrlHelperFactory))).Returns(urlHelperFactoryMock.Object);
        var controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
                //RequestServices = serviceProviderMock.Object
            }
        };
        return controllerContext;
    }
}
