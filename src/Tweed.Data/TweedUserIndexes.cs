using Raven.Client.Documents.Indexes;
using Tweed.Data.Entities;

namespace Tweed.Data;

public class
    TweedUsers_FollowerCount : AbstractIndexCreationTask<TweedUser, TweedUsers_FollowerCount.Result>
{
    public TweedUsers_FollowerCount()
    {
        Map = appUsers => from appUser in appUsers
            from follow in appUser.Follows
            select new
            {
                UserId = follow.LeaderId,
                FollowerCount = 0
            };

        Reduce = results => from result in results
            group result by result.UserId
            into g
            select new
            {
                UserId = g.Key,
                FollowerCount = g.Sum(x => x.FollowerCount + 1)
            };
    }

    public class Result
    {
        public string? UserId { get; set; }

        public int FollowerCount { get; set; }
    }
}
