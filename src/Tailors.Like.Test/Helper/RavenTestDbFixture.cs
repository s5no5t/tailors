using Raven.Client.Documents;
using Raven.TestDriver;
using Tailors.Like.Infrastructure;
using Tailors.Thread.Infrastructure;
using Tailors.Tweed.Infrastructure;
using Xunit;

namespace Tailors.Like.Test.Helper;

public class RavenTestDbFixture : RavenTestDriver
{
    public IDocumentStore CreateDocumentStore()
    {
        var store = GetDocumentStore();
        store.DeployTweedIndexes();
        return store;
    }

    protected override void PreInitialize(IDocumentStore documentStore)
    {
        documentStore.PreInitializeLikes();
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