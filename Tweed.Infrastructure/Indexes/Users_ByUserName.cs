using Raven.Client.Documents.Indexes;
using Tweed.Domain.Model;

namespace Tweed.Infrastructure.Indexes;

public class Users_ByUserName : AbstractIndexCreationTask<User>
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
