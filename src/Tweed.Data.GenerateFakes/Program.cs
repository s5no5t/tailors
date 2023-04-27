using Microsoft.Extensions.Configuration;
using Raven.DependencyInjection;
using Tweed.Data.GenerateFakes;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var ravenSettings = config.GetRequiredSection("RavenSettings").Get<RavenSettings>();
using var store = DocumentStoreHelper.OpenDocumentStore(ravenSettings);

var dataFakerSettings =
    config.GetRequiredSection("DataFakerSettings").Get<DataFakerSettings>();
var dataFaker = new DataFaker(store, dataFakerSettings);

var appUsers = await dataFaker.CreateFakeAppUsers();
await dataFaker.CreateFakeFollows(appUsers);
var tweeds = await dataFaker.CreateFakeTweeds(appUsers);
await dataFaker.CreateFakeLikes(appUsers, tweeds);
