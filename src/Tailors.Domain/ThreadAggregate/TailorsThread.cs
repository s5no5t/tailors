using System.Collections.ObjectModel;
using Newtonsoft.Json;
using OneOf;
using OneOf.Types;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Domain.ThreadAggregate;

public record MaxDepthReachedError(string Message);

public class TailorsThread(string? id = null, string? parentThreadId = null)
{
    private const int MaxDepth = 64;
    public const int MaxTweedReferenceDepth = MaxDepth - 1;

    [JsonConstructor]
    public TailorsThread(string id, TweedOrThreadReference? root, string? parentThreadId) : this(id, parentThreadId)
    {
        Root = root;
    }

    public string? Id { get; set; } = id;
    public string? ParentThreadId { get; } = parentThreadId;
    public TweedOrThreadReference? Root { get; private set; }

    public class TweedOrThreadReference
    {
        private readonly List<TweedOrThreadReference> _replies = new();

        public TweedOrThreadReference(string? tweedId, string? threadId)
        {
            if (tweedId is null && threadId is null)
                throw new ArgumentException("Either tweedId or threadId must be provided");
            if (tweedId is not null && threadId is not null)
                throw new ArgumentException("Either tweedId or threadId must be provided, not both");

            TweedId = tweedId;
            ThreadId = threadId;
        }

        public string? TweedId { get; }

        public string? ThreadId { get; }

        public IReadOnlyList<TweedOrThreadReference> Replies => _replies;

        internal void AddReply(TweedOrThreadReference reference)
        {
            _replies.Add(reference);
        }
    }

    public OneOf<Success, MaxDepthReachedError> AddTweed(Tweed tweed)
    {
        if (tweed.Id is null)
            throw new ArgumentException($"Tweed {tweed.Id} is missing Id");

        if (tweed.ThreadId is null)
            throw new ArgumentException($"Tweed {tweed.Id} is missing ThreadId");

        if (tweed.ThreadId != Id)
            throw new ArgumentException($"Tweed {tweed.Id} already belongs to thread {tweed.ThreadId}");

        // This is a root Tweed
        if (Root is null)
        {
            Root = new TweedOrThreadReference(tweed.Id, null);
            return new Success();
        }

        // This is a reply to a reply
        var path = FindTweedPath(tweed.ParentTweedId!);
        if (path.Count == 0)
            throw new ArgumentException($"Parent tweed {tweed.ParentTweedId} of tweed {tweed.Id} not found in thread {Id}");
        if (path.Count == MaxTweedReferenceDepth)
            return new MaxDepthReachedError($"Max tweed ref depth of {MaxTweedReferenceDepth} reached for thread {Id}");
        var parentTweedRef = path.Last();
        parentTweedRef.AddReply(new TweedOrThreadReference(tweed.Id, null));
        return new Success();
    }

    public OneOf<Success, MaxDepthReachedError> AddChildThreadReference(string parentTweedId, string childThreadId)
    {
        var path = FindTweedPath(parentTweedId);
        if (path.Count == 0)
            throw new ArgumentException($"Tweed {parentTweedId} not found in thread {Id}");
        if (path.Count == MaxDepth)
            return new MaxDepthReachedError($"Max depth of {MaxTweedReferenceDepth} reached for thread {Id}");
        var parentTweedRef = path.Last();
        parentTweedRef.AddReply(new TweedOrThreadReference(null, childThreadId));
        return new Success();
    }

    public ReadOnlyCollection<TweedOrThreadReference> FindTweedPath(string tweedId)
    {
        if (Root is null)
            return new ReadOnlyCollection<TweedOrThreadReference>(Array.Empty<TweedOrThreadReference>());

        Queue<List<TweedOrThreadReference>> queue = new();
        queue.Enqueue(new List<TweedOrThreadReference>
        {
            Root
        });

        while (queue.Count > 0)
        {
            var currentPath = queue.Dequeue();
            var currentRef = currentPath.Last();

            if (currentRef.TweedId == tweedId)
                return currentPath.AsReadOnly();

            foreach (var reply in currentRef.Replies)
            {
                var replyPath = new List<TweedOrThreadReference>(currentPath) { reply };
                queue.Enqueue(replyPath);
            }
        }

        return new ReadOnlyCollection<TweedOrThreadReference>(Array.Empty<TweedOrThreadReference>());
    }

    public int GetThreadDepth(string tweedId)
    {
        var path = FindTweedPath(tweedId);
        return path.Count;
    }
}
