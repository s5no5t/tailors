namespace Tailors.Tweed.Domain;

public record DomainError(string Message)
{
    public DomainError() : this("")
    {
    }
};
