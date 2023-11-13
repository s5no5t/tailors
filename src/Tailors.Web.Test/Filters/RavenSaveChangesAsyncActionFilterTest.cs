using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Tailors.Web.Filters;
using Xunit;

namespace Tailors.Web.Test.Filters;

public class RavenSaveChangesAsyncActionFilterTest
{
    [Fact]
    public async Task OnActionExecutionAsync_ShouldCallSaveChangesAsync()
    {
        var sessionMock = new SessionMock();

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new PageActionDescriptor(),
            new ModelStateDictionary());

        var executingContext = new ActionExecutingContext(
            actionContext,
            Array.Empty<IFilterMetadata>(),
            new Dictionary<string, object>()!,
            new object());

        var sut = new RavenSaveChangesAsyncActionFilter(sessionMock);

        var executedContext =
            new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object());

        await sut.OnActionExecutionAsync(executingContext,
            () => Task.FromResult(executedContext));

        Assert.True(sessionMock.ChangesSaved);
    }
}
