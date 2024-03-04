using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tailors.Web.Helper;

namespace Tailors.Web.Test.TestHelper;

internal static class ControllerTestHelper
{
    internal static ClaimsPrincipal BuildPrincipal(string userId)
    {
        List<Claim> claims = [new Claim(ClaimsPrincipalExtensions.UrnTailorsAppUserId, userId)];
        var identity = new ClaimsIdentity(claims);
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
