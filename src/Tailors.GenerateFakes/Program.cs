using Microsoft.Extensions.Configuration;
using Raven.DependencyInjection;
using Tailors.Domain.ThreadAggregate;
using Tailors.GenerateFakes;
using Tailors.Infrastructure.ThreadAggregate;
using Tailors.Infrastructure.TweedAggregate;

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

foreach (var tweed in tweeds)
{
    using var session = store.OpenAsyncSession();
    var threadUseCase = new ThreadUseCase(new ThreadRepository(session), new TweedRepository(session));
    await threadUseCase.AddTweedToThread(tweed.Id!);
    await session.SaveChangesAsync();
}

await dataFaker.CreateFakeLikes(users, tweeds);
