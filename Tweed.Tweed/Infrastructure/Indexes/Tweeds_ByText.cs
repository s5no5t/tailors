using Raven.Client.Documents.Indexes;
using Tweed.Tweed.Domain;

namespace Tweed.Tweed.Infrastructure.Indexes;

public class Tweeds_ByText : AbstractIndexCreationTask<TheTweed>
{
    public Tweeds_ByText()
    {
        Map = tweeds => from tweed in tweeds
            select new
            {
                tweed.Id,
                tweed.Text
            };

        Analyzers.Add(a => a.Text, "StandardAnalyzer");
    }
}