using Raven.Client.Documents;
using Raven.DependencyInjection;
using Tailors.Like.Infrastructure;
using Tailors.User.Infrastructure;

namespace Tailors.GenerateFakes;

internal static class DocumentStoreHelper
{
    internal static IDocumentStore OpenDocumentStore(RavenSettings ravenSettings)
    {
        var documentStore = new DocumentStore
        {
            Urls = ravenSettings.Urls,
            Database = ravenSettings.DatabaseName
        };

        documentStore.PreInitialize();
        documentStore.Initialize();
        documentStore.EnsureDatabaseExists();

        return documentStore;
    }
}
