using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;
using Raven.Client.Documents.Session;
using Tweed.Web.Filters;
using Xunit;

namespace Tweed.Web.Test.Filters;

public class RavenSaveChangesAsyncPageFilterTest
{
    [Fact]
    public async Task OnPageHandlerExecutionAsync_ShouldCallSaveChangesAsync()
    {
        var sessionMock = new Mock<IAsyncDocumentSession>();
        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };
        var pageContext = new PageContext(actionContext);
        var model = new Mock<PageModel>();
        var pageHandlerExecutingContext = new PageHandlerExecutingContext(pageContext,
            Array.Empty<IFilterMetadata>(), null, new Dictionary<string, object?>(), model.Object);
        var pageHandlerExecutedContext = new PageHandlerExecutedContext(pageContext,
            Array.Empty<IFilterMetadata>(), null, model.Object);
        var asyncFilter = new RavenSaveChangesAsyncPageFilter(sessionMock.Object);
        
        await asyncFilter.OnPageHandlerExecutionAsync(pageHandlerExecutingContext,
            () => Task.FromResult(pageHandlerExecutedContext));
        
        sessionMock.Verify(s => s.SaveChangesAsync(default));
    }
}
