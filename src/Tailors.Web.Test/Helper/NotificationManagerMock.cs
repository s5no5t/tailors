using Tailors.Web.Helper;

namespace Tailors.Web.Test.Helper;

public class NotificationManagerMock : INotificationManager
{
    public string? SuccessMessage { get; private set; }

    public void AppendSuccess(string message)
    {
        SuccessMessage = message;
    }

    public void Clear()
    {
        SuccessMessage = null;
    }
}
