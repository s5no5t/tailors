using Raven.Client.Documents.Indexes;
using Tweed.Data.Entities;

namespace Tweed.Data;

public class IdentityUsers_ByUserName : AbstractIndexCreationTask<TweedIdentityUser>
{
    public IdentityUsers_ByUserName()
    {
        Map = users => from user in users
                       select new
                       {
                           user.UserName
                       };
        Index(u => u.UserName, FieldIndexing.Search);
    }
}
