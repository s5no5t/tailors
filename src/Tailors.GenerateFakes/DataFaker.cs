using Bogus;
using Raven.Client.Documents;
using Tailors.Like.Domain;
using Tailors.Thread.Domain.ThreadAggregate;
using Tailors.Thread.Domain.TweedAggregate;
using Tailors.User.Domain;

namespace Tailors.GenerateFakes;

internal class DataFaker
{
    private readonly IDocumentStore _documentStore;
    private readonly DataFakerSettings _settings;

    public DataFaker(IDocumentStore documentStore, DataFakerSettings settings)
    {
        _documentStore = documentStore;
        _settings = settings;
    }

    internal async Task<List<AppUser>> CreateFakeUsers()
    {
        await using var bulkInsert = _documentStore.BulkInsert();

        var userFaker = new Faker<AppUser>()
            .RuleFor(u => u.UserName, (f, _) => f.Internet.UserName())
            .RuleFor(u => u.Email, (f, _) => f.Internet.ExampleEmail());

        var numberOfUsers = _settings.NumberOfUsers;
        var users = userFaker.Generate(numberOfUsers);
        foreach (var user in users) await bulkInsert.StoreAsync(user);
        Console.WriteLine("{0} Users created", users.Count);

        return users;
    }

    internal async Task CreateFakeFollows(List<AppUser> users)
    {
        await using var bulkInsert = _documentStore.BulkInsert();

        var followsFaker = new Faker<UserFollows.LeaderReference>()
            .RuleFor(f => f.LeaderId, f => f.PickRandom(users).Id)
            .RuleFor(f => f.CreatedAt, f => f.Date.Past());

        var userFollowsFaker = new Faker<UserFollows>()
            .RuleFor(u => u.Follows, f => followsFaker.GenerateBetween(0, users.Count - 1));

        foreach (var user in users)
        {
            var userFollows = userFollowsFaker.Generate(1).First();
            userFollows.UserId = user.Id;
            await bulkInsert.StoreAsync(userFollows);
        }

        Console.WriteLine("{0} UserFollows created", users.Count);
    }

    internal async Task<List<Tweed>> CreateFakeTweeds(List<AppUser> users)
    {
        await using var bulkInsert = _documentStore.BulkInsert();

        var threadFaker = new Faker<TweedThread>();

        var numThreads = _settings.NumberOfTweeds;
        var threads = threadFaker.Generate(numThreads);
        foreach (var thread in threads) await bulkInsert.StoreAsync(thread);

        var tweedFaker = new Faker<Tweed>()
            .RuleFor(t => t.CreatedAt, f => f.Date.Past())
            .RuleFor(t => t.Text, f => f.Lorem.Paragraph(1))
            .RuleFor(t => t.AuthorId, f => f.PickRandom(users).Id)
            .RuleFor(t => t.ThreadId, f => threads[f.IndexFaker].Id);

        var numTweeds = _settings.NumberOfTweeds;
        var tweeds = tweedFaker.Generate(numTweeds);
        foreach (var tweed in tweeds) await bulkInsert.StoreAsync(tweed);
        Console.WriteLine("{0} Tweeds created", tweeds.Count);

        return tweeds;
    }

    internal async Task CreateFakeLikes(List<AppUser> users, List<Tweed> tweeds)
    {
        await using var bulkInsert = _documentStore.BulkInsert();

        var likesFaker = new Faker<UserLikes.TweedLike>()
            .RuleFor(f => f.TweedId, f => f.PickRandom(tweeds).Id)
            .RuleFor(f => f.CreatedAt, f => f.Date.Past());

        foreach (var user in users)
        {
            var likes = likesFaker.GenerateBetween(0, tweeds.Count - 1);
            var userLikes = new UserLikes(user.Id, likes);
           
            await bulkInsert.StoreAsync(userLikes);
        }

        Console.WriteLine("{0} UserLikes created", users.Count);
    }
}

internal class DataFakerSettings
{
    public int NumberOfUsers { get; set; }
    public int NumberOfTweeds { get; set; }
}
