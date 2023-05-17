namespace Tweed.Thread.Domain;

public record ReferenceNotFoundError(string Message)
{
    public ReferenceNotFoundError() : this("")
    {
    }
};
