using Raven.Client.Documents.Indexes;
using Tailors.Domain.AppUser;

namespace Tailors.Infrastructure.UserFollows.Indexes;

public class UsersByUserName : AbstractIndexCreationTask<AppUser>
{
    public UsersByUserName()
    {
        Map = users => from user in users
                       select new
                       {
                           user.UserName
                       };
        Index(u => u.UserName, FieldIndexing.Search);
    }
}
