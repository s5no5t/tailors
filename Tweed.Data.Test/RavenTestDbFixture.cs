using Raven.Client.Documents;
using Raven.TestDriver;

namespace Tweed.Data.Test;

public class RavenTestDbFixture : RavenTestDriver
{
    public IDocumentStore CreateDocumentStore()
    {
        return GetDocumentStore();
    }

    protected override void PreInitialize(IDocumentStore documentStore)
    {
        documentStore.Conventions.ThrowIfQueryPageSizeIsNotSet = true;
    }
}
