using Raven.Client.Documents.Indexes;
using Tailors.User.Domain.AppUser;

namespace Tailors.User.Infrastructure.Indexes;

public class Users_ByUserName : AbstractIndexCreationTask<AppUser>
{
    public Users_ByUserName()
    {
        Map = users => from user in users
                       select new
                       {
                           user.UserName
                       };
        Index(u => u.UserName, FieldIndexing.Search);
    }
}
