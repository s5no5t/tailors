using Tweed.Web.Views.Shared;

namespace Tweed.Web.Views.Tweed;

public class GetByIdViewModel
{
    public GetByIdViewModel(TweedViewModel tweed)
    {
        Tweed = tweed;
    }

    public TweedViewModel Tweed { get; set; }
}
