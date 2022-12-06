// See https://aka.ms/new-console-template for more information

using Bogus;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using Raven.DependencyInjection;
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
await using var bulkInsert = store.BulkInsert();

var appUsersFaker = new Faker<AppUser>()
    .RuleFor(u => u.UserName, (f, _) => f.Internet.UserName());

var appUsers = appUsersFaker.Generate(10);

foreach (var appUser in appUsers)
{
    await bulkInsert.StoreAsync(appUser);
    Console.WriteLine("AppUser {0} created", appUser.UserName);
}
