using Tweed.Data.Model;
using Xunit;

namespace Tweed.Data.Test.Model;

public class AppUserFollowsTest
{
    [Fact]
    public void BuildId_ShouldReturnCombinedId()
    {
        var appUserId = "AppUser/123-A";
        
        var followsId = AppUserFollows.BuildId(appUserId);
        
        Assert.Equal($"{appUserId}/Follows", followsId);
    } 
}
