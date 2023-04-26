// See https://aka.ms/new-console-template for more information

using Bogus;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Raven.Client.Documents;
using Raven.DependencyInjection;
using Tweed.Data;
using Tweed.Data.Entities;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Development.json")
    .Build();

using var store = OpenDocumentStore(config);

await using var bulkInsert = store.BulkInsert();

var appUserFaker = new Faker<AppUser>()
    .RuleFor(u => u.UserName, (f, _) => f.Internet.UserName())
    .RuleFor(u => u.Email, (f, _) => f.Internet.ExampleEmail());

var numAppUsers = config.GetValue<int>("NumberOfAppUsers");
var appUsers = appUserFaker.Generate(numAppUsers);
foreach (var appUser in appUsers) await bulkInsert.StoreAsync(appUser);
Console.WriteLine("{0} AppUser instances created", appUsers.Count);

using (var session = store.OpenAsyncSession())
{
    var followsFaker = new Faker<Follows>()
        .RuleFor(f => f.LeaderId, f => f.PickRandom(appUsers).Id)
        .RuleFor(f => f.CreatedAt, f => dateTimeToZonedDateTime(f.Date.Past()));

    var appUserFollowsFaker = new Faker<AppUserFollows>()
        .RuleFor(u => u.Follows, f => followsFaker.GenerateBetween(0, appUsers.Count - 1));

    foreach (var appUser in appUsers)
    {
        var appUserFollows = appUserFollowsFaker.Generate(1).First();
        appUserFollows.AppUserId = appUser.Id;
        await session.StoreAsync(appUserFollows);
    }

    await session.SaveChangesAsync();
    Console.WriteLine("{0} AppUserFollows instances created", appUsers.Count);
}

var tweedFaker = new Faker<Tweed.Data.Entities.Tweed>()
    .RuleFor(t => t.CreatedAt, f => dateTimeToZonedDateTime(f.Date.Past()))
    .RuleFor(t => t.Text, f => f.Lorem.Paragraph(1))
    .RuleFor(t => t.AuthorId, f => f.PickRandom(appUsers).Id);

var numTweeds = config.GetValue<int>("NumberOfTweeds");
var tweeds = tweedFaker.Generate(numTweeds);
foreach (var tweed in tweeds) await bulkInsert.StoreAsync(tweed);
Console.WriteLine("{0} Tweed instances created", tweeds.Count);

using (var session = store.OpenAsyncSession())
{
    var likesFaker = new Faker<TweedLike>()
        .RuleFor(f => f.TweedId, f => f.PickRandom(tweeds).Id)
        .RuleFor(f => f.CreatedAt, f => dateTimeToZonedDateTime(f.Date.Past()));

    var appUserLikesFaker = new Faker<AppUser>()
        .RuleFor(u => u.Likes, f => likesFaker.GenerateBetween(0, tweeds.Count - 1));

    foreach (var appUser in appUsers)
    {
        appUserLikesFaker.Populate(appUser);
        await session.StoreAsync(appUser);
    }

    await session.SaveChangesAsync();
}

Console.WriteLine("{0} AppUser instances updated with likes", appUsers.Count);

ZonedDateTime dateTimeToZonedDateTime(DateTime dateTime)
{
    var localDate = LocalDateTime.FromDateTime(dateTime);
    var berlinTimeZone = DateTimeZoneProviders.Tzdb["UTC"];
    var timeZonedDateTime = berlinTimeZone.AtLeniently(localDate);
    return timeZonedDateTime;
}

IDocumentStore OpenDocumentStore(IConfigurationRoot configurationRoot)
{
    var ravenSettings = configurationRoot.GetRequiredSection("RavenSettings").Get<RavenSettings>();

    var documentStore = new DocumentStore
    {
        Urls = ravenSettings.Urls,
        Database = ravenSettings.DatabaseName
    };

    documentStore.PreInitialize();
    documentStore.Initialize();
    documentStore.EnsureDatabaseExists();

    return documentStore;
}
