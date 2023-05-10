using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Tweed.Domain.Model;
using Tweed.Domain.Test.Helper;
using Tweed.Infrastructure;
using Xunit;

namespace Tweed.Domain.Test;

[Collection("RavenDb Collection")]
public class TweedThreadServiceTest
{
    private readonly IDocumentStore _store;

    public TweedThreadServiceTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }
    
    [Fact]
    public async Task GetLeadingTweeds_ShouldReturnNull_WhenTweedIsntFound()
    {
        using var session = _store.OpenAsyncSession();
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = "rootTweedId"
            }
        };
        await session.StoreAsync(thread);
        TweedThreadService service = new(session);

        var leadingTweeds = await service.GetLeadingTweeds("threadId", "unknownTweedId");

        Assert.Null(leadingTweeds);
    }

    [Fact]
    public async Task GetLeadingTweeds_ShouldReturnEmptyList_WhenThereIsOnlyRoot()
    {
        using var session = _store.OpenAsyncSession();
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = "rootTweedId"
            }
        };
        await session.StoreAsync(thread);
        TweedThreadService service = new(session);

        var leadingTweeds = await service.GetLeadingTweeds("threadId", "rootTweedId");

        Assert.NotNull(leadingTweeds);
        Assert.Empty(leadingTweeds);
    }

    [Fact]
    public async Task GetLeadingTweeds_ShouldReturnLeadingTweed_WhenThereIsOneLeadingTweed()
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
                        TweedId = "tweedId"
                    }
                }
            }
        };
        await session.StoreAsync(thread);
        TweedThreadService service = new(session);

        var leadingTweeds = await service.GetLeadingTweeds("threadId", "tweedId");

        Assert.NotNull(leadingTweeds);
        Assert.Equal("rootTweedId", leadingTweeds[0].TweedId);
    }

    [Fact]
    public async Task GetLeadingTweeds_ShouldReturnLeadingTweeds_WhenThereIsAnotherBranch()
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
                        TweedId = "parentTweedId",
                        Replies = new List<TweedThread.TweedReference>
                        {
                            new()
                            {
                                TweedId = "tweedId"
                            }
                        }
                    },
                    new()
                    {
                        TweedId = "otherTweedId"
                    }
                }
            }
        };
        await session.StoreAsync(thread);
        TweedThreadService service = new(session);

        var leadingTweeds = await service.GetLeadingTweeds("threadId", "tweedId");

        Assert.NotNull(leadingTweeds);
        Assert.Equal("rootTweedId", leadingTweeds[0].TweedId);
        Assert.Equal("parentTweedId", leadingTweeds[1].TweedId);
    }

    [Fact]
    public void AddTweedToThread_ShouldSetRootTweed_IfParentTweedIdIsNull()
    {
        using var session = _store.OpenAsyncSession();
        TweedThread thread = new();
        TweedThreadService service = new(session);

        service.AddTweedToThread(thread, "tweedId", null);

        Assert.Equal("tweedId", thread.Root.TweedId);
    }

    [Fact]
    public void AddTweedToThread_ShouldInsertReplyToRootTweed()
    {
        using var session = _store.OpenAsyncSession();
        TweedThread thread = new()
        {
            Id = "threadId",
            Root = new TweedThread.TweedReference
            {
                TweedId = "rootTweedId"
            }
        };
        TweedThreadService service = new(session);

        service.AddTweedToThread(thread, "tweedId", "rootTweedId");

        Assert.Equal("tweedId", thread.Root.Replies[0].TweedId);
    }

    [Fact]
    public void AddTweedToThread_ShouldInsertReplyToReplyTweed()
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
                        TweedId = "replyTweedId"
                    }
                }
            }
        };
        TweedThreadService service = new(session);

        service.AddTweedToThread(thread, "tweedId", "replyTweedId");

        Assert.Equal("tweedId", thread.Root.Replies[0].Replies[0].TweedId);
    }
}
