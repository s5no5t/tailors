namespace Tweed.Data.Model;

public class TweedThread
{
    public string? RootTweedId { get; set; }

    public List<TweedThreadReply> Replies { get; set; } = new();
}

public class TweedThreadReply
{
    public string? TweedId { get; set; }

    public List<TweedThreadReply> Replies { get; set; } = new();
}