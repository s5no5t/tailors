using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web.Pages;
using Tweed.Web.Test.TestHelper;
using Xunit;

namespace Tweed.Web.Test.Pages;

public class IndexModelTest
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    public IndexModelTest()
    {
        _userManagerMock = UserManagerMockHelper.MockUserManager<AppUser>();
    }

    [Fact]
    public async Task OnGet_ShouldLoadTweeds()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var indexModel = new IndexModel(tweedQueriesMock.Object, _userManagerMock.Object);
        await indexModel.OnGetAsync();
        tweedQueriesMock.Verify(t => t.GetLatestTweeds());
    }
}
