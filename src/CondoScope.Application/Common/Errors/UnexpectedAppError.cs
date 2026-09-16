using FluentResults;

namespace CondoScope.Application.Common.Errors;

public class UnexpectedAppError : ExceptionalError
{
    public UnexpectedAppError(Exception ex) : base($"An unexpected error occurred.", ex)
    {
        WithMetadata("ErrorCode", "APP_UNEXPECTED_ERROR");
        WithMetadata("TimeStamp", DateTime.UtcNow);
    }
}
