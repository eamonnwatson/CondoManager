using FluentResults;

namespace CondoScope.Domain.Errors;

public class InvalidEmailError : Error
{
    public string Email { get; }
    public InvalidEmailError(string email) : base($"'{email}' is not a valid email address.")
    {
        Email = email;

        WithMetadata("ErrorCode", "DOM_INVALID_EMAIL");
        WithMetadata("TimeStamp", DateTime.UtcNow);
    }

}
