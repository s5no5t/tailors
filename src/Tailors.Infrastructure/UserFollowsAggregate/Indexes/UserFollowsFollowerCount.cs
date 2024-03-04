using Raven.Client.Documents.Indexes;
using Tailors.Domain.UserFollowsAggregate;

namespace Tailors.Infrastructure.UserFollowsAggregate.Indexes;

public class
    UserFollowsFollowerCount : AbstractIndexCreationTask<UserFollows,
    UserFollowsFollowerCount.Result>
{
    public UserFollowsFollowerCount()
    {
        Map = users => from user in users
                       from follow in user.Follows
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
