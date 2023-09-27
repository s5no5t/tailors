using System.Collections.ObjectModel;
using Newtonsoft.Json;
using OneOf;
using OneOf.Types;
using Tailors.Tweed.Domain;

namespace Tailors.Thread.Domain;

public class TailorsThread
{
    public TailorsThread(string? id = null)
    {
        Id = id;
    }
    
    [JsonConstructor]
    public TailorsThread(string id, TweedReference? root)
    {
        Id = id;
        Root = root;
    }

    public string? Id { get; }
    public TweedReference? Root { get; private set; }

    public class TweedReference
    {
        public TweedReference(string? tweedId)
        {
            TweedId = tweedId;
        }

        public string? TweedId { get; }

        public List<TweedReference> Replies { get; } = new();
    }
    
    public OneOf<Success, ThreadError, TweedError> AddTweed(TailorsTweed tweed)
    {
        if (tweed.Id is null)
            return new TweedError($"Tweed {tweed.Id} is missing Id");

        // This is a root Tweed
        if (tweed.ParentTweedId is null)
        {
            Root = new TweedReference(tweed.Id);
            return new Success();
        }

        // This is a reply to a reply
        var path = FindTweedPath(tweed.ParentTweedId);
        if (path.Count == 0)
            return new ThreadError($"Tweed {tweed.ParentTweedId} not found in thread {Id}");
        var parentTweedRef = path.Last();
        parentTweedRef.Replies.Add(new TweedReference(tweed.Id));
        return new Success();
    }

    public ReadOnlyCollection<TweedReference> FindTweedPath(string tweedId)
    {
        if (Root is null)
            return new ReadOnlyCollection<TweedReference>(Array.Empty<TweedReference>());
        
        Queue<List<TweedReference>> queue = new();
        queue.Enqueue(new List<TweedReference>
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
                var replyPath = new List<TweedReference>(currentPath) { reply };
                queue.Enqueue(replyPath);
            }
        }

        return new ReadOnlyCollection<TweedReference>(Array.Empty<TweedReference>());
    }
}