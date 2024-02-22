using Raven.Client.Documents.Indexes;
using Tailors.Domain.TweedAggregate;

namespace Tailors.Infrastructure.TweedAggregate.Indexes;

public class TweedsByLeadingTweedIdsString : AbstractIndexCreationTask<Tweed>
{
    public class Result
    {
        public string LeadingTweedIdsString { get; set; } = string.Empty;
    }

    public TweedsByLeadingTweedIdsString()
    {
        Map = tweeds => from tweed in tweeds
                        select new Result
                        {
                            LeadingTweedIdsString = string.Join(",", tweed.LeadingTweedIds)
                        };
    }
}
