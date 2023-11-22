using Tailors.Domain.UserAggregate;
using Tailors.Infrastructure.Test.Helper;
using Tailors.Infrastructure.UserAggregate;

namespace Tailors.Infrastructure.Test.UserAggregate;

[Trait("Category", "Integration")]
[Collection("RavenDB")]
public class UserRepositoryTest(RavenTestDbFixture ravenDb)
{
    [Fact]
    public async Task SearchUsers_ShouldReturnEmptyList_WhenNoResults()
    {
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
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
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
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
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
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
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
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
