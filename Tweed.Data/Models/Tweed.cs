using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace Tweed.Data.Models;

public class Tweed
{
    [Required] [StringLength(280)] public string? Content { get; set; }
    public LocalDateTime CreatedAt { get; set; }
}
