namespace Tailors.Thread.Domain;

public record DomainError(string Message)
{
    public DomainError() : this("")
    {
    }
};
