using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Tweed.Data.Domain;
using Tweed.Data.Model;
using Tweed.Data.Test.Helper;
using Xunit;

namespace Tweed.Data.Test.Domain;

[Collection("RavenDb Collection")]
public class TweedThreadServiceTest
{
    private readonly IDocumentStore _store;

    public TweedThreadServiceTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task AddTweedToThread_ShouldCreateThread_IfItDoesntExist()
    {
        using var session = _store.OpenAsyncSession();
        TweedThreadService service = new(session);

        await service.AddReplyToThread("threadId", "tweedId", "rootTweedId");

        var thread = await session.LoadAsync<TweedThread>("threadId");
        Assert.Equal("threadId", thread.Id);
        Assert.Equal("rootTweedId", thread.Root.TweedId);
        Assert.Equal("tweedId", thread.Root.Replies[0].TweedId);
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToRootTweed()
    {
        using var session = _store.OpenAsyncSession();
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedReference
            {
                TweedId = "rootTweedId"
            }
        };
        await session.StoreAsync(thread);
        TweedThreadService service = new(session);

        await service.AddReplyToThread("threadId", "tweedId", "rootTweedId");

        await session.LoadAsync<TweedThread>("threadId");
        Assert.Contains("tweedId", thread.Root.Replies.Select(r => r.TweedId));
    }

    [Fact]
    public async Task AddTweedToThread_ShouldInsertReplyToReplyTweed()
    {
        using var session = _store.OpenAsyncSession();
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedReference
            {
                TweedId = "rootTweedId",
                Replies = new List<TweedReference>
                {
                    new()
                    {
                        TweedId = "replyTweedId"
                    }
                }
            }
        };
        await session.StoreAsync(thread);
        TweedThreadService service = new(session);

        await service.AddReplyToThread("threadId", "tweedId", "replyTweedId");

        await session.LoadAsync<TweedThread>("threadId");

        Assert.Equal("tweedId", thread.Root.Replies[0].Replies[0].TweedId);
    }
}
