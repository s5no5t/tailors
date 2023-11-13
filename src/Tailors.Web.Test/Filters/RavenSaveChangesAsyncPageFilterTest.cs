using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Tailors.Web.Filters;
using Xunit;

namespace Tailors.Web.Test.Filters;

public class RavenSaveChangesAsyncPageFilterTest
{
    [Fact]
    public async Task OnPageHandlerExecutionAsync_ShouldCallSaveChangesAsync()
    {
        var sessionMock = new AsyncDocumentSessionMock();
        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };
        var pageContext = new PageContext(actionContext);
        var pageHandlerExecutingContext = new PageHandlerExecutingContext(pageContext,
            Array.Empty<IFilterMetadata>(), null, new Dictionary<string, object?>(), new object());
        var pageHandlerExecutedContext = new PageHandlerExecutedContext(pageContext,
            Array.Empty<IFilterMetadata>(), null, new object());
        var asyncFilter = new RavenSaveChangesAsyncPageFilter(sessionMock);

        await asyncFilter.OnPageHandlerExecutionAsync(pageHandlerExecutingContext,
            () => Task.FromResult(pageHandlerExecutedContext));

        Assert.True(sessionMock.ChangesSaved);
    }
}