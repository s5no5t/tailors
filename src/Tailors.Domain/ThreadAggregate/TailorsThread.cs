using System.Collections.ObjectModel;
using Newtonsoft.Json;
using OneOf;
using OneOf.Types;

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

        internal TweedOrThreadReference(string? tweedId, string? threadId)
        {
            if (tweedId is null && threadId is null)
                throw new ArgumentException("Either tweedId or threadId must be provided");
            if (tweedId is not null && threadId is not null)
                throw new ArgumentException("Either tweedId or threadId must be provided, not both");

            TweedId = tweedId;
            ThreadId = threadId;
        }

        [JsonConstructor]
        public TweedOrThreadReference(string? tweedId, string? threadId, List<TweedOrThreadReference> replies) : this(
            tweedId, threadId)
        {
            _replies = replies;
        }

        public string? TweedId { get; }

        public string? ThreadId { get; }

        public IReadOnlyList<TweedOrThreadReference> Replies => _replies;

        internal void AddReplyTweed(string tweedId)
        {
            _replies.Add(new TweedOrThreadReference(tweedId, null));
        }

        internal void AddReplyChildThread(string childThreadId)
        {
            _replies.Add(new TweedOrThreadReference(null, childThreadId));
        }
    }

    public OneOf<Success, MaxDepthReachedError> AddTweed(string tweedId, string? parentTweedId = null)
    {
        // This is a root Tweed
        if (Root is null)
        {
            Root = new TweedOrThreadReference(tweedId, null);
            return new Success();
        }

        if (parentTweedId is null) throw new ArgumentNullException($"Parent tweed id is null but must not be when tweed {tweedId} isn't root");

        // This is a reply to a reply
        var path = FindTweedPath(parentTweedId);
        if (path.Count == 0)
            throw new ArgumentException(
                $"Parent tweed {parentTweedId} of tweed {tweedId} not found in thread {Id}");
        if (path.Count == MaxTweedReferenceDepth)
            return new MaxDepthReachedError($"Max tweed ref depth of {MaxTweedReferenceDepth} reached for thread {Id}");
        var parentTweedRef = path.Last();
        parentTweedRef.AddReplyTweed(tweedId);
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
        parentTweedRef.AddReplyChildThread(childThreadId);
        return new Success();
    }

    public ReadOnlyCollection<TweedOrThreadReference> FindTweedPath(string tweedId)
    {
        if (Root is null)
            return new ReadOnlyCollection<TweedOrThreadReference>(Array.Empty<TweedOrThreadReference>());

        Queue<List<TweedOrThreadReference>> queue = new();
        queue.Enqueue([Root]);

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
