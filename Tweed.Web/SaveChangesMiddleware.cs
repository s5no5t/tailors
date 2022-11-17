using Raven.Client.Documents.Session;

namespace Tweed.Web;

public class SaveChangesMiddleware
{
    private readonly RequestDelegate _next;

    public SaveChangesMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAsyncDocumentSession session)
    {
        await _next(context);
        await session.SaveChangesAsync();
    }
}
