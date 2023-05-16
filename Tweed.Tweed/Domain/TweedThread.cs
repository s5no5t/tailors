namespace Tweed.Tweed.Domain;

public class TweedThread
{
    public string? Id { get; set; }
    public TweedReference Root { get; set; } = new();

    public class TweedReference
    {
        public string? TweedId { get; set; }

        public List<TweedReference> Replies { get; set; } = new();
    }
}
