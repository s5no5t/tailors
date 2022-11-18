using System.Threading.Tasks;
using Moq;
using Tweed.Data;
using Tweed.Web.Pages;
using Xunit;

namespace Tweed.Web.Test;

public class IndexModelTest
{
    [Fact]
    public async Task OnGet_ShouldLoadTweeds()
    {
        var tweedQueriesMock = new Mock<ITweedQueries>();
        var indexModel = new IndexModel(tweedQueriesMock.Object);
        await indexModel.OnGetAsync();
        tweedQueriesMock.Verify(t => t.GetLatestTweeds());
    }
}
