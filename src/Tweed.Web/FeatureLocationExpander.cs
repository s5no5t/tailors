using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Razor;

// Code taken from https://scottsauber.com/2016/04/25/feature-folder-structure-in-asp-net-core/

// These annotations make Resharper not complain about not finding the Views.
// ASP.NET Core support coming to R# for Feature Folders soon – https://youtrack.jetbrains.com/issue/RSRP-461882
[assembly: AspMvcViewLocationFormat("/Features/{1}/{0}.cshtml")]
[assembly: AspMvcViewLocationFormat("/Features/Shared/{0}.cshtml")]
[assembly: AspMvcPartialViewLocationFormat("~/Features/{1}/{0}.cshtml")]
[assembly: AspMvcPartialViewLocationFormat("~/Features/Shared/{0}.cshtml")]
namespace Tweed.Web
{
    public class FeatureFolderLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // Don't need anything here, but required by the interface
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            // The old locations are /Views/{1}/{0}.cshtml and /Views/Shared/{0}.cshtml where {1} is the controller and {0} is the name of the View
            // Replace /Views with /Features
            return new[]
            {
                "/Features/{1}/{0}.cshtml",
                "/Features/Shared/{0}.cshtml"
            };
        }
    }
}