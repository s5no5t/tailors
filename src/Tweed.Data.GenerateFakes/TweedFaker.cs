using Bogus;
using Raven.Client.Documents;
using Tweed.Data.Model;

namespace Tweed.Data.GenerateFakes;

internal class DataFaker
{
    private readonly IDocumentStore _documentStore;
    private readonly DataFakerSettings _settings;

    public DataFaker(IDocumentStore documentStore, DataFakerSettings settings)
    {
        _documentStore = documentStore;
        _settings = settings;
    }

    internal async Task<List<AppUser>> CreateFakeAppUsers()
    {
        await using var bulkInsert = _documentStore.BulkInsert();

        var appUserFaker = new Faker<AppUser>()
            .RuleFor(u => u.UserName, (f, _) => f.Internet.UserName())
            .RuleFor(u => u.Email, (f, _) => f.Internet.ExampleEmail());

        var numAppUsers = _settings.NumberOfAppUsers;
        var appUsers = appUserFaker.Generate(numAppUsers);
        foreach (var appUser in appUsers) await bulkInsert.StoreAsync(appUser);
        Console.WriteLine("{0} AppUser instances created", appUsers.Count);

        return appUsers;
    }

    internal async Task CreateFakeFollows(List<AppUser> appUsers)
    {
        await using var bulkInsert = _documentStore.BulkInsert();

        var followsFaker = new Faker<Follows>()
            .RuleFor(f => f.LeaderId, f => f.PickRandom(appUsers).Id)
            .RuleFor(f => f.CreatedAt, f => DateHelper.DateTimeToZonedDateTime(f.Date.Past()));

        var appUserFollowsFaker = new Faker<AppUserFollows>()
            .RuleFor(u => u.Follows, f => followsFaker.GenerateBetween(0, appUsers.Count - 1));

        foreach (var appUser in appUsers)
        {
            var appUserFollows = appUserFollowsFaker.Generate(1).First();
            appUserFollows.AppUserId = appUser.Id;
            await bulkInsert.StoreAsync(appUserFollows);
        }

        Console.WriteLine("{0} AppUserFollows instances created", appUsers.Count);
    }

    internal async Task<List<Model.Tweed>> CreateFakeTweeds(List<AppUser> appUsers)
    {
        await using var bulkInsert = _documentStore.BulkInsert();

        var tweedFaker = new Faker<Model.Tweed>()
            .RuleFor(t => t.CreatedAt, f => DateHelper.DateTimeToZonedDateTime(f.Date.Past()))
            .RuleFor(t => t.Text, f => f.Lorem.Paragraph(1))
            .RuleFor(t => t.AuthorId, f => f.PickRandom(appUsers).Id);

        var numTweeds = _settings.NumberOfTweeds;
        var tweeds = tweedFaker.Generate(numTweeds);
        foreach (var tweed in tweeds) await bulkInsert.StoreAsync(tweed);
        Console.WriteLine("{0} Tweed instances created", tweeds.Count);

        return tweeds;
    }

    internal async Task CreateFakeLikes(List<AppUser> appUsers, List<Model.Tweed> tweeds)
    {
        await using var bulkInsert = _documentStore.BulkInsert();

        var likesFaker = new Faker<TweedLike>()
            .RuleFor(f => f.TweedId, f => f.PickRandom(tweeds).Id)
            .RuleFor(f => f.CreatedAt, f => DateHelper.DateTimeToZonedDateTime(f.Date.Past()));

        var appUserLikesFaker = new Faker<AppUserLikes>()
            .RuleFor(u => u.Likes, f => likesFaker.GenerateBetween(0, tweeds.Count - 1));

        foreach (var appUser in appUsers)
        {
            var appUserLikes = appUserLikesFaker.Generate(1).First();
            appUserLikes.AppUserId = appUser.Id;
            await bulkInsert.StoreAsync(appUserLikes);
        }

        Console.WriteLine("{0} AppUser instances updated with likes", appUsers.Count);
    }
}

internal class DataFakerSettings
{
    public int NumberOfAppUsers { get; set; }
    public int NumberOfTweeds { get; set; }
}
