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

var ravenSettings = config.GetRequiredSection("RavenSettings").Get<RavenSettings>();

using var store = new DocumentStore
{
    Urls = ravenSettings.Urls,
    Database = ravenSettings.DatabaseName
}.Initialize();

store.EnsureDatabaseExists();

await using var bulkInsert = store.BulkInsert();

var appUserFaker = new Faker<AppUser>()
    .RuleFor(u => u.UserName, (f, _) => f.Internet.UserName());

var appUsers = appUserFaker.Generate(5);
foreach (var appUser in appUsers)
{
    await bulkInsert.StoreAsync(appUser);
    Console.WriteLine("AppUser {0} created", appUser.UserName);
}

var tweedFaker = new Faker<Tweed.Data.Entities.Tweed>()
    .RuleFor(t => t.CreatedAt, f => dateTimeToZonedDateTime(f.Date.Past()))
    .RuleFor(t => t.Text, f => f.Lorem.Paragraph(1))
    .RuleFor(t => t.AuthorId, f => f.PickRandom(appUsers).Id);

var tweeds = tweedFaker.Generate(5);
foreach (var tweed in tweeds)
{
    await bulkInsert.StoreAsync(tweed);
    Console.WriteLine("Tweed {0} created", tweed.Text);
}

ZonedDateTime dateTimeToZonedDateTime(DateTime dateTime)
{
    var localDate = LocalDateTime.FromDateTime(dateTime);
    var berlinTimeZone = DateTimeZoneProviders.Tzdb["Europe/Berlin"];
    var timeZonedDateTime = berlinTimeZone.AtStrictly(localDate);
    return timeZonedDateTime;
}
