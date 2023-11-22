using Raven.Client.Documents;
using Testcontainers.RavenDb;

namespace Tailors.Infrastructure.Test.Helper;

// ReSharper disable once ClassNeverInstantiated.Global
public class RavenTestDbFixture : IAsyncLifetime
{
    public IDocumentStore DocumentStore { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var ravenDbContainer = new RavenDbBuilder()
            .WithImage("ravendb/ravendb:latest")
            .Build();
        await ravenDbContainer.StartAsync();

        DocumentStore = new DocumentStore
        {
            Urls = new[] { ravenDbContainer.GetConnectionString() },
            Database = "Tailors"
        };

        DocumentStore.PreInitialize();
        DocumentStore.Initialize();
        DocumentStore.EnsureDatabaseExists();
        DocumentStore.DeployIndexes();
    }

    public Task DisposeAsync()
    {
        DocumentStore.Dispose();
        return Task.CompletedTask;
    }
}

[CollectionDefinition("RavenDB")]
public class RavenDbCollection : ICollectionFixture<RavenTestDbFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
