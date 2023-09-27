using Raven.Client.Documents;
using Tailors.Tweed.Infrastructure.Indexes;

namespace Tailors.Tweed.Infrastructure;

public static class RavenDbSetup
{
    public static void DeployTweedIndexes(this IDocumentStore store)
    {
        new TweedsByAuthorIdAndCreatedAt().Execute(store);
        new TweedsByText().Execute(store);
    }
}