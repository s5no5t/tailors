using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Raven.Client.Documents;
using Tweed.Data.Entities;
using Xunit;

namespace Tweed.Data.Test;

[Collection("RavenDb Collection")]
public class IdentityUserQueriesTest
{
    private static readonly ZonedDateTime FixedZonedDateTime =
        new(new LocalDateTime(2022, 11, 18, 15, 20), DateTimeZone.Utc, new Offset());

    private readonly IDocumentStore _store;

    public IdentityUserQueriesTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task Search_ShouldReturnEmptyList_WhenNoResults()
    {
        using var session = _store.OpenAsyncSession();
        await session.StoreAsync(new TweedIdentityUser
        {
            UserName = "UserName"
        });
        await session.SaveChangesAsync();
        IdentityUserQueries queries = new(session);

        var results = await queries.Search("noresults");

        Assert.Empty(results);
    }

    [Fact]
    public async Task Search_ShouldFindMatchingAppUser()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        await session.StoreAsync(new TweedIdentityUser
        {
            UserName = "UserName"
        });
        await session.SaveChangesAsync();
        IdentityUserQueries queries = new(session);

        var results = await queries.Search("UserName");

        Assert.Contains(results, u => u.UserName == "UserName");
    }

    [Fact]
    public async Task Search_ShouldFindMatchingAppUser_WhenUserNamePrefixGiven()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        await session.StoreAsync(new TweedIdentityUser
        {
            UserName = "UserName"
        });
        await session.SaveChangesAsync();
        IdentityUserQueries queries = new(session);

        var results = await queries.Search("Use");

        Assert.Contains(results, u => u.UserName == "UserName");
    }

    [Fact]
    public async Task Search_ShouldReturn20Users()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        for (var i = 0; i < 21; i++)
        {
            await session.StoreAsync(new TweedIdentityUser
            {
                UserName = $"User-{i}"
            });
            await session.SaveChangesAsync();
        }

        IdentityUserQueries queries = new(session);

        var results = await queries.Search("User");

        Assert.Equal(20, results.Count);
    }
}
