using Microsoft.Extensions.Configuration;
using Raven.DependencyInjection;
using Tweed.GenerateFakes;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var ravenSettings = config.GetRequiredSection("RavenSettings").Get<RavenSettings>();
using var store = DocumentStoreHelper.OpenDocumentStore(ravenSettings);

var dataFakerSettings =
    config.GetRequiredSection("DataFakerSettings").Get<DataFakerSettings>();
var dataFaker = new DataFaker(store, dataFakerSettings);

var users = await dataFaker.CreateFakeUsers();
await dataFaker.CreateFakeFollows(users);
var tweeds = await dataFaker.CreateFakeTweeds(users);
await dataFaker.CreateFakeLikes(users, tweeds);
