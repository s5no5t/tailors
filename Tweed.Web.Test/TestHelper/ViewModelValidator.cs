using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Tweed.Web.Test.TestHelper;

public static class ViewModelValidator
{
    public static void Validate(this PageModel viewModel)
    {
        var validationContext = new ValidationContext(viewModel, null, null);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(viewModel, validationContext, validationResults, true);
        foreach (var validationResult in validationResults)
            viewModel.ModelState.AddModelError(
                validationResult.MemberNames.FirstOrDefault() ?? string.Empty,
                validationResult.ErrorMessage!);
    }
}
