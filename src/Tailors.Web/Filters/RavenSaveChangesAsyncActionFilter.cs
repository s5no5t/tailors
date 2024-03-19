using Microsoft.AspNetCore.Mvc.Filters;
using Raven.Client.Documents.Session;

namespace Tailors.Web.Filters;

public class RavenSaveChangesAsyncActionFilter(IAsyncDocumentSession dbSession) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        await next();

        await dbSession.SaveChangesAsync();
    }
}
