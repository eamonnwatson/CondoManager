using FluentResults;

namespace CondoManager.Domain.Errors;

public class InvalidEmailError : Error
{
    public string Email { get; }
    public InvalidEmailError(string email) : base($"'{email}' is not a valid email address.")
    {
        Email = email;

        WithMetadata("ErrorCode", "INVALID_EMAIL");
        WithMetadata("TimeStamp", DateTime.UtcNow);
    }

}
