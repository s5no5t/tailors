using Raven.Client.Documents.Indexes;

namespace Tweed.User.Infrastructure.Indexes;

public class Users_ByUserName : AbstractIndexCreationTask<Domain.AppUser>
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
