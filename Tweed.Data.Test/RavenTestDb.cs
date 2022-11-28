using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.TestDriver;

namespace Tweed.Data.Test;

public class RavenTestDb : RavenTestDriver
{
    private readonly IDocumentStore _store;

    public RavenTestDb()
    {
        _store = GetDocumentStore();
    }

    public IAsyncDocumentSession OpenAsyncSession => _store.OpenAsyncSession();

    protected override void PreInitialize(IDocumentStore documentStore)
    {
        documentStore.Conventions.ThrowIfQueryPageSizeIsNotSet = true;
    }
}
