using Raven.Client.Documents;
using Raven.TestDriver;
using Tweed.Infrastructure.Setup;
using Xunit;

namespace Tweed.Infrastructure.Test.Helper;

public class RavenTestDbFixture : RavenTestDriver
{
    public IDocumentStore CreateDocumentStore()
    {
        var store = GetDocumentStore();
        store.DeployIndexes();
        return store;
    }

    protected override void PreInitialize(IDocumentStore documentStore)
    {
        documentStore.PreInitialize();
        documentStore.Conventions.ThrowIfQueryPageSizeIsNotSet = true;
    }
}

[CollectionDefinition("RavenDB")]
public class RavenDbCollection : ICollectionFixture<RavenTestDbFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
