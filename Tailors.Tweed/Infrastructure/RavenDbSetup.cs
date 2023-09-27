using Raven.Client.Documents;
using Tailors.Tweed.Infrastructure.Indexes;

namespace Tailors.Tweed.Infrastructure;

public static class RavenDbSetup
{
    public static void PreInitializeTweeds(this IDocumentStore store)
    {
        store.ApplyCustomConventions();
    }

    private static void ApplyCustomConventions(this IDocumentStore store)
    {
    }

    public static void DeployTweedIndexes(this IDocumentStore store)
    {
        new Tweeds_ByAuthorIdAndCreatedAt().Execute(store);
        new Tweeds_ByText().Execute(store);
    }
}