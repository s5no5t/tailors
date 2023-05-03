namespace Tweed.Data.Model;

public class Thread
{
    public string? RootTweedId { get; set; }

    public List<ThreadTweedReference> Replies { get; set; } = new();
}

public class ThreadTweedReference
{
    public string? TweedId { get; set; }

    public List<ThreadTweedReference> Replies { get; set; } = new();
}
