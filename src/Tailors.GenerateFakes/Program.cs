using Microsoft.Extensions.Configuration;
using Raven.DependencyInjection;
using Tailors.GenerateFakes;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var ravenSettings = config.GetRequiredSection("RavenSettings").Get<RavenSettings>();
if (ravenSettings is null)
    throw new ArgumentNullException(nameof(ravenSettings));
using var store = DocumentStoreHelper.OpenDocumentStore(ravenSettings);

var dataFakerSettings = config.GetRequiredSection("DataFakerSettings").Get<DataFakerSettings>();
if (dataFakerSettings is null)
    throw new ArgumentNullException(nameof(dataFakerSettings));

var dataFaker = new DataFaker(store, dataFakerSettings);

var users = await dataFaker.CreateFakeUsers();
await dataFaker.CreateFakeFollows(users);
var tweeds = await dataFaker.CreateFakeTweeds(users);

await dataFaker.CreateFakeLikes(users, tweeds);
