using Bogus;
using Raven.Client.Documents;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserFollowsAggregate;
using Tailors.Domain.UserLikesAggregate;

namespace Tailors.GenerateFakes;

internal class DataFaker(IDocumentStore documentStore, DataFakerSettings settings)
{
    private readonly Faker _faker = new();

    internal async Task<List<AppUser>> CreateFakeUsers()
    {
        await using var bulkInsert = documentStore.BulkInsert();

        var userFaker = new Faker<AppUser>()
            .RuleFor(u => u.UserName, (f, _) => f.Internet.UserName())
            .RuleFor(u => u.Email, (f, _) => f.Internet.ExampleEmail());

        var numberOfUsers = settings.NumberOfUsers;
        var users = userFaker.Generate(numberOfUsers);
        foreach (var user in users) await bulkInsert.StoreAsync(user);
        Console.WriteLine("{0} Users created", users.Count);

        return users;
    }

    internal async Task CreateFakeFollows(List<AppUser> users)
    {
        await using var bulkInsert = documentStore.BulkInsert();
        var f = new Faker();

        foreach (var user in users)
        {
            var userFollows = new UserFollows(user.Id!);

            for (var i = 0; i < f.Random.Int(0, users.Count - 1); i++)
                userFollows.AddFollows(f.PickRandom(users).Id!, f.Date.Past());

            await bulkInsert.StoreAsync(userFollows);
        }

        Console.WriteLine("{0} UserFollows created", users.Count);
    }

    internal async Task<List<Tweed>> CreateFakeTweeds(List<AppUser> users)
    {
        await using var bulkInsert = documentStore.BulkInsert();
        var numTweeds = settings.NumberOfTweeds;

        List<Tweed> tweeds = [];
        for (var i = 0; i < numTweeds; i++)
        {
            var tweed = new Tweed(_faker.PickRandom(users).Id!, _faker.Lorem.Paragraph(1), _faker.Date.Past());

            var parentTweed = tweeds.Count > 0 && _faker.Random.Bool() ? tweeds[i - 1] : null;
            if (parentTweed is not null)
            {
                tweed.AddLeadingTweedIds(parentTweed.LeadingTweedIds);
                tweed.AddLeadingTweedId(parentTweed.Id!);
            }

            await bulkInsert.StoreAsync(tweed);
            tweeds.Add(tweed);
        }

        Console.WriteLine("{0} Tweeds created", numTweeds);

        return tweeds;
    }

    internal async Task CreateFakeLikes(List<AppUser> users, List<Tweed> tweeds)
    {
        await using var bulkInsert = documentStore.BulkInsert();

        foreach (var user in users)
        {
            var userLikes = new UserLikes(user.Id!);

            var numLikes = _faker.Random.Int(0, tweeds.Count - 1);
            for (var i = 0; i < numLikes; i++) userLikes.AddLike(_faker.PickRandom(tweeds).Id!, _faker.Date.Past());

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
