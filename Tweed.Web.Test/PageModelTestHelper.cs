using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace Tweed.Web.Test;

internal static class PageModelTestHelper
{
    internal static ClaimsPrincipal BuildPrincipal()
    {
        var displayName = "User name";
        var identity = new GenericIdentity(displayName);
        var principal = new ClaimsPrincipal(identity);
        return principal;
    }

    internal static PageContext BuildPageContext(ClaimsPrincipal user)
    {
// use default context with user
        var httpContext = new DefaultHttpContext
        {
            User = user
        };
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);

        var pageContext = new PageContext(actionContext);
        return pageContext;
    }
}
