using Raven.Client.Documents.Indexes;

namespace Tweed.Thread.Infrastructure.Indexes;

public class Tweeds_ByText : AbstractIndexCreationTask<Domain.Tweed>
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