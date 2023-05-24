namespace Tailors.Thread.Domain;

public record ReferenceNotFoundError(string Message)
{
    public ReferenceNotFoundError() : this("")
    {
    }
};
