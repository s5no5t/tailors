using Microsoft.Extensions.Configuration;
using Tweed.Data.GenerateFakes;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Development.json")
    .Build();

using var store = DocumentStoreHelper.OpenDocumentStore(config);

var dataFaker = new DataFaker(store, config);

var appUsers = await dataFaker.CreateFakeAppUsers();
await dataFaker.CreateFakeFollows(appUsers);
var tweeds = await dataFaker.CreateFakeTweeds(appUsers);
await dataFaker.CreateFakeLikes(appUsers, tweeds);
