using System.Collections.Generic;
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
    public async Task InsertTweedIntoThread_ShouldCreateThread_IfItDoesntExist()
    {
        using var session = _store.OpenAsyncSession();
        TweedThreadService service = new(session);

        var thread = await service.InsertTweedIntoThread("tweedId", "parentTweedId");

        Assert.Equal("parentTweedId", thread.Root.TweedId);
        Assert.Equal("tweedId", thread.Root.Replies[0].TweedId);
    }

    [Fact]
    public async Task InsertTweedIntoThread_ShouldInsertReplyToRootTweed()
    {
        using var session = _store.OpenAsyncSession();
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = "parentTweedId"
            }
        };
        await session.StoreAsync(thread);
        TweedThreadService service = new(session);

        await service.InsertTweedIntoThread("tweedId", "parentTweedId");

        await session.LoadAsync<TweedThread>("threadId");
        Assert.Equal("tweedId", thread.Root.Replies[0].TweedId);
    }

    [Fact]
    public async Task InsertTweedIntoThread_ShouldInsertReplyToReplyTweed()
    {
        using var session = _store.OpenAsyncSession();
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = "rootTweedId",
                Replies = new List<TweedThread.TweedReference>
                {
                    new()
                    {
                        TweedId = "parentTweedId"
                    }
                }
            }
        };
        await session.StoreAsync(thread);
        TweedThreadService service = new(session);

        await service.InsertTweedIntoThread("tweedId", "parentTweedId");

        await session.LoadAsync<TweedThread>("threadId");

        Assert.Equal("tweedId", thread.Root.Replies[0].Replies[0].TweedId);
    }
}