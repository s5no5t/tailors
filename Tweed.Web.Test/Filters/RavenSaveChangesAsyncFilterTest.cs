using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Moq;
using Raven.Client.Documents.Session;
using Tweed.Web.Filters;
using Xunit;

namespace Tweed.Web.Test.Filters;

public class RavenSaveChangesAsyncFilterTest
{
    [Fact]
    public async Task InvokeAsync()
    {
        var sessionMock = new Mock<IAsyncDocumentSession>();

        var pageContext = new PageContext(new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new PageActionDescriptor(),
            new ModelStateDictionary()));
        var model = new Mock<PageModel>();

        var pageHandlerExecutingContext = new PageHandlerExecutingContext(
            pageContext,
            Array.Empty<IFilterMetadata>(),
            new HandlerMethodDescriptor(),
            new Dictionary<string, object>()!,
            model.Object);
        var pageHandlerExecutedContext = new PageHandlerExecutedContext(
            pageContext,
            Array.Empty<IFilterMetadata>(),
            new HandlerMethodDescriptor(),
            model.Object);
        PageHandlerExecutionDelegate next = () => Task.FromResult(pageHandlerExecutedContext);

        var asyncFilter = new RavenSaveChangesAsyncFilter(sessionMock.Object);

        await asyncFilter.OnPageHandlerExecutionAsync(pageHandlerExecutingContext, next);

        sessionMock.Verify(s => s.SaveChangesAsync(default));
    }
}
