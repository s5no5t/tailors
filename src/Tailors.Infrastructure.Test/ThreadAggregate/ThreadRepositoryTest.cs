using JetBrains.Annotations;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.TweedAggregate;
using Tailors.Infrastructure.Test.Helper;
using Tailors.Infrastructure.ThreadAggregate;

namespace Tailors.Infrastructure.Test.ThreadAggregate;

[Trait("Category", "Integration")]
[Collection("RavenDB")]
public class ThreadRepositoryTest(RavenTestDbFixture ravenDb) : IClassFixture<RavenTestDbFixture>
{
    [Fact]
    public async Task GetById_ShouldReturnThread()
    {
        using (var session = ravenDb.DocumentStore.OpenAsyncSession())
        {
            TailorsThread thread = new("test");
            thread.AddTweed("tweedId");
            thread.AddTweed("tweedId2", "tweedId");
            await session.StoreAsync(thread);
            await session.SaveChangesAsync();
        }

        using (var session = ravenDb.DocumentStore.OpenAsyncSession())
        {
            var repository = new ThreadRepository(session);
            var thread2 = await repository.GetById("test");
            thread2.Switch(
                [AssertionMethod] (t) => { Assert.Equal("tweedId2", t.Root!.Replies[0].TweedId); }, _ => Assert.Fail());
        }
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNone()
    {
        using var session = ravenDb.DocumentStore.OpenAsyncSession();
        var repository = new ThreadRepository(session);

        var thread = await repository.GetById("invalid");
        thread.Switch(
            _ => Assert.Fail("Should not return a thread"),
            _ => { });
    }
}
