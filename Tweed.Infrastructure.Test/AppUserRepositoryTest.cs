using Raven.Client.Documents;
using Tweed.Domain.Model;
using Tweed.Infrastructure.Test.Helper;
using Xunit;

namespace Tweed.Infrastructure.Test;

[Collection("RavenDb Collection")]
public class AppUserRepositoryTest
{
    private readonly IDocumentStore _store;

    public AppUserRepositoryTest(RavenTestDbFixture ravenDb)
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
        AppUserRepository repository = new(session);

        var results = await repository.Search("noresults");

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
        AppUserRepository repository = new(session);

        var results = await repository.Search("UserName");

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
        AppUserRepository repository = new(session);

        var results = await repository.Search("Use");

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

        AppUserRepository repository = new(session);

        var results = await repository.Search("User");

        Assert.Equal(20, results.Count);
    }
}