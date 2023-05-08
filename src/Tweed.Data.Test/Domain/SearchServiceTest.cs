using System.Threading.Tasks;
using Raven.Client.Documents;
using Tweed.Data.Domain;
using Tweed.Data.Model;
using Tweed.Data.Test.Helper;
using Xunit;

namespace Tweed.Data.Test.Domain;

[Collection("RavenDb Collection")]
public class SearchServiceTest
{
    private readonly IDocumentStore _store;

    public SearchServiceTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task SearchAppUsers_ShouldReturnEmptyList_WhenNoResults()
    {
        using var session = _store.OpenAsyncSession();
        await session.StoreAsync(new AppUser
        {
            UserName = "UserName"
        });
        await session.SaveChangesAsync();
        SearchService service = new(session);

        var results = await service.SearchAppUsers("noresults");

        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAppUsers_ShouldFindMatchingAppUser()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        await session.StoreAsync(new AppUser
        {
            UserName = "UserName"
        });
        await session.SaveChangesAsync();
        SearchService service = new(session);

        var results = await service.SearchAppUsers("UserName");

        Assert.Contains(results, u => u.UserName == "UserName");
    }

    [Fact]
    public async Task SearchAppUsers_ShouldFindMatchingAppUser_WhenUserNamePrefixGiven()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        await session.StoreAsync(new AppUser
        {
            UserName = "UserName"
        });
        await session.SaveChangesAsync();
        SearchService service = new(session);

        var results = await service.SearchAppUsers("Use");

        Assert.Contains(results, u => u.UserName == "UserName");
    }

    [Fact]
    public async Task SearchAppUsers_ShouldReturn20Users()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        for (var i = 0; i < 21; i++)
        {
            await session.StoreAsync(new AppUser
            {
                UserName = $"User-{i}"
            });
            await session.SaveChangesAsync();
        }

        SearchService service = new(session);

        var results = await service.SearchAppUsers("User");

        Assert.Equal(20, results.Count);
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
        Data.Model.Tweed tweed = new()
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
