using Raven.Client.Documents.Indexes;
using Tweed.Domain.Model;

namespace Tweed.Domain.Indexes;

public class AppUsers_ByUserName : AbstractIndexCreationTask<AppUser>
{
    public AppUsers_ByUserName()
    {
        Map = users => from user in users
                       select new
                       {
                           user.UserName
                       };
        Index(u => u.UserName, FieldIndexing.Search);
    }
}
