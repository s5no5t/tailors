using FluentResults;

namespace Tweed.Thread.Domain;

public class ReferenceNotFoundError : Error
{
    public ReferenceNotFoundError(string?message) : base(message)
    {
    }
    
    public ReferenceNotFoundError()
    {
    }
}