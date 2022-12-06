// See https://aka.ms/new-console-template for more information

using Bogus;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Raven.Client.Documents;
using Raven.Client.NodaTime;
using Raven.DependencyInjection;
using Tweed.Data;
using Tweed.Data.Entities;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Development.json")
    .Build();

using var store = OpenDocumentStore(config);

await using var bulkInsert = store.BulkInsert();

var appUserFaker = new Faker<AppUser>()
    .RuleFor(u => u.UserName, (f, _) => f.Internet.UserName());

var numAppUsers = config.GetValue<int>("NumberOfAppUsers");
var appUsers = appUserFaker.Generate(numAppUsers);
foreach (var appUser in appUsers) await bulkInsert.StoreAsync(appUser);
Console.WriteLine("{0} AppUser instances created", appUsers.Count);

using (var session = store.OpenAsyncSession())
{
    var followsFaker = new Faker<Follows>()
        .RuleFor(f => f.LeaderId, f => f.PickRandom(appUsers).Id)
        .RuleFor(f => f.CreatedAt, f => dateTimeToZonedDateTime(f.Date.Past()));

    var appUserFollowsFaker = new Faker<AppUser>()
        .RuleFor(u => u.Follows, f => followsFaker.GenerateBetween(0, appUsers.Count - 1));

    foreach (var appUser in appUsers)
    {
        appUserFollowsFaker.Populate(appUser);
        await session.StoreAsync(appUser);
    }

    await session.SaveChangesAsync();
}

Console.WriteLine("{0} AppUser instances updated with followers", appUsers.Count);

var tweedFaker = new Faker<Tweed.Data.Entities.Tweed>()
    .RuleFor(t => t.CreatedAt, f => dateTimeToZonedDateTime(f.Date.Past()))
    .RuleFor(t => t.Text, f => f.Lorem.Paragraph(1))
    .RuleFor(t => t.AuthorId, f => f.PickRandom(appUsers).Id)
    .RuleFor(t => t.Likes, f => f.PickRandom(appUsers, f.Random.Number(appUsers.Count - 1))
        .Select(a => new Like
        {
            CreatedAt = dateTimeToZonedDateTime(f.Date.Past()),
            UserId = f.PickRandom(appUsers).Id
        }).ToList());

var numTweeds = config.GetValue<int>("NumberOfTweeds");
var tweeds = tweedFaker.Generate(numTweeds);
foreach (var tweed in tweeds) await bulkInsert.StoreAsync(tweed);
Console.WriteLine("{0} Tweed instances created", tweeds.Count);

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

    documentStore.ConfigureForNodaTime();
    documentStore.Initialize();
    documentStore.EnsureDatabaseExists();

    return documentStore;
}
