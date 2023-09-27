using System.Threading.Tasks;
using Raven.Client.Documents;
using Tailors.User.Domain.AppUser;
using Tailors.User.Infrastructure;
using Tailors.User.Test.Helper;
using Xunit;

namespace Tailors.User.Test.Infrastructure;

[Trait("Category","Integration")]
[Collection("RavenDB")]
public class UserRepositoryTest
{
    private readonly IDocumentStore _store;

    public UserRepositoryTest(RavenTestDbFixture ravenDb)
    {
        _store = ravenDb.CreateDocumentStore();
    }

    [Fact]
    public async Task SearchUsers_ShouldReturnEmptyList_WhenNoResults()
    {
        using var session = _store.OpenAsyncSession();
        await session.StoreAsync(new AppUser
        {
            UserName = "UserName"
        });
        await session.SaveChangesAsync();
        UserRepository repository = new(session);

        var results = await repository.Search("noresults");

        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchUsers_ShouldFindMatchingUser()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        await session.StoreAsync(new AppUser
        {
            UserName = "UserName"
        });
        await session.SaveChangesAsync();
        UserRepository repository = new(session);

        var results = await repository.Search("UserName");

        Assert.Contains(results, u => u.UserName == "UserName");
    }

    [Fact]
    public async Task SearchUsers_ShouldFindMatchingUser_WhenUserNamePrefixGiven()
    {
        using var session = _store.OpenAsyncSession();
        session.Advanced.WaitForIndexesAfterSaveChanges();
        await session.StoreAsync(new AppUser
        {
            UserName = "UserName"
        });
        await session.SaveChangesAsync();
        UserRepository repository = new(session);

        var results = await repository.Search("Use");

        Assert.Contains(results, u => u.UserName == "UserName");
    }

    [Fact]
    public async Task SearchUsers_ShouldReturn20Users()
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

        UserRepository repository = new(session);

        var results = await repository.Search("User");

        Assert.Equal(20, results.Count);
    }
}
