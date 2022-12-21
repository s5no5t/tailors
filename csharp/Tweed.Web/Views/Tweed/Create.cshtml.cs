using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Tweed.Web.Views.Tweed;

public class CreateViewModel
{
    [BindProperty]
    [Required]
    [StringLength(280)]
    public string Text { get; set; } = "";
}
