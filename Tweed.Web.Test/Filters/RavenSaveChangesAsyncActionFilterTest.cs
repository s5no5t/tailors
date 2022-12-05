using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;
using Raven.Client.Documents.Session;
using Tweed.Web.Filters;
using Xunit;

namespace Tweed.Web.Test.Filters;

public class RavenSaveChangesAsyncActionFilterTest
{
    [Fact]
    public async Task OnActionExecutionAsync_ShouldCallSaveChangesAsync()
    {
        var sessionMock = new Mock<IAsyncDocumentSession>();

        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new PageActionDescriptor(),
            new ModelStateDictionary());
        var model = new Mock<PageModel>();

        var executingContext = new ActionExecutingContext(
            actionContext,
            Array.Empty<IFilterMetadata>(),
            new Dictionary<string, object>()!,
            model.Object);

        var asyncFilter = new RavenSaveChangesAsyncActionFilter(sessionMock.Object);

        var controller = new Mock<Controller>();

        var executedContext =
            new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller.Object);

        await asyncFilter.OnActionExecutionAsync(executingContext,
            () => Task.FromResult(executedContext));

        sessionMock.Verify(s => s.SaveChangesAsync(default));
    }
}
