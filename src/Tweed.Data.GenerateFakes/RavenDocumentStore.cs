using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using Raven.Client.NodaTime;
using Raven.DependencyInjection;

namespace Tweed.Data.GenerateFakes;

public class RavenDocumentStore
{
    internal static IDocumentStore OpenDocumentStore(IConfigurationRoot configurationRoot)
    {
        var ravenSettings = configurationRoot.GetRequiredSection("RavenSettings").Get<RavenSettings>();

        var documentStore = new DocumentStore
        {
            Urls = ravenSettings.Urls,
            Database = ravenSettings.DatabaseName
        };

        documentStore.ConfigureForNodaTime();
        documentStore.Initialize();
        documentStore.EnsureDatabaseExists();

        return documentStore;
    }
}
