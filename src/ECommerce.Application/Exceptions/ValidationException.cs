namespace ECommerce.Application.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(Dictionary<string, string[]> errors) : base("The request is invalid.")
    {
        Errors = errors;
    }

    public Dictionary<string, string[]> Errors { get; }
}
