using System.Threading.Tasks;
using Raven.Client.Documents;
using Tweed.Domain.Test.Helper;
using Tweed.Infrastructure;
using Xunit;

namespace Tweed.Domain.Test;

[Collection("RavenDb Collection")]
public class SearchServiceTest
{
    private readonly IDocumentStore _store;

    public SearchServiceTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task Search_ShouldReturnEmptyList_WhenNoResults()
    {
        using var session = _store.OpenAsyncSession();
        var queries = new SearchService(session);

        var tweeds = await queries.SearchTweeds("noresults");

        Assert.Empty(tweeds);
    }

    [Fact]
    public async Task Search_ShouldReturnResult_WhenTermIsFound()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        Domain.Model.Tweed tweed = new()
        {
            Id = "tweedId",
            Text = "Here is a word included."
        };
        await session.StoreAsync(tweed);
        await session.SaveChangesAsync();
        var queries = new SearchService(session);

        var tweeds = await queries.SearchTweeds("word");

        Assert.Equal("tweedId", tweeds[0].Id);
    }
}