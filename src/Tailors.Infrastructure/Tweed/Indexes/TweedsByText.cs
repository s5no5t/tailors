using Raven.Client.Documents.Indexes;
using Tailors.Domain.Tweed;

namespace Tailors.Tweed.Infrastructure.Indexes;

public class TweedsByText : AbstractIndexCreationTask<TailorsTweed>
{
    public TweedsByText()
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