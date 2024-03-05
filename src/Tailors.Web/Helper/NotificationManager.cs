namespace Tailors.Web.Helper;

public interface INotificationManager
{
    string? SuccessMessage { get; }
    void AppendSuccess(string message);
    void Clear();
}

public sealed class NotificationManager(IHttpContextAccessor httpContextAccessor) : INotificationManager
{
    private const string NotificationSuccessCookieName = "notification-success";

    public string? SuccessMessage
    {
        get
        {
            httpContextAccessor.HttpContext!.Request.Cookies.TryGetValue(
                NotificationSuccessCookieName,
                out var successMessage);
            return successMessage;
        }
    }

    public void AppendSuccess(string message)
    {
        httpContextAccessor.HttpContext!.Response.Cookies.Append(NotificationSuccessCookieName,
            message);
    }

    public void Clear()
    {
        httpContextAccessor.HttpContext!.Response.Cookies.Delete(NotificationSuccessCookieName);
    }
}
