using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tweed.Web.Test.TestHelper;
using Tweed.Web.Views.Tweed;
using Xunit;

namespace Tweed.Web.Test.Views;

public class CreateViewModelTest
{
    [Fact]
    public void ValidatesTextRequired()
    {
        CreateViewModel viewModel = new()
        {
            Text = ""
        };
        var result = viewModel.Validate();

        Assert.Equal(ModelValidationState.Invalid, result["text"]!.ValidationState);
    }

    [Fact]
    public void ValidatesTextTooLong()
    {
        CreateViewModel viewModel = new()
        {
            Text = new string('a', 281)
        };

        var result = viewModel.Validate();

        Assert.Equal(ModelValidationState.Invalid, result["text"]!.ValidationState);
    }
}
