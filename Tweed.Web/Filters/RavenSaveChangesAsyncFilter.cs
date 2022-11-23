using Microsoft.AspNetCore.Mvc.Filters;
using Raven.Client.Documents.Session;

namespace Tweed.Web.Filters;

public class RavenSaveChangesAsyncFilter : IAsyncPageFilter
{
    private readonly IAsyncDocumentSession _dbSession;

    public RavenSaveChangesAsyncFilter(IAsyncDocumentSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        await Task.CompletedTask;
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var result = await next.Invoke();

        if (result.Exception == null && !result.Canceled) await _dbSession.SaveChangesAsync();
    }
}
