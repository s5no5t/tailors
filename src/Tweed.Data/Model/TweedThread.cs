namespace Tweed.Data.Model;

public class TweedThread
{
    public string? Id { get; set; }
    public TweedReference Root { get; set; } = new();

    public static string BuildId(string rootTweedId)
    {
        ArgumentNullException.ThrowIfNull(rootTweedId);
        return $"{rootTweedId}/Thread";
    }

    public class TweedReference
    {
        public string? TweedId { get; set; }

        public List<TweedReference> Replies { get; set; } = new();
    }
}