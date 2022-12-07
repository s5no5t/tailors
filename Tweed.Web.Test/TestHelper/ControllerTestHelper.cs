using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tweed.Web.Test.TestHelper;

internal static class ControllerTestHelper
{
    internal static ClaimsPrincipal BuildPrincipal()
    {
        var displayName = "User name";
        var identity = new GenericIdentity(displayName);
        var principal = new ClaimsPrincipal(identity);
        return principal;
    }

    internal static ControllerContext BuildControllerContext(ClaimsPrincipal user)
    {
        var controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };
        return controllerContext;
    }
}
