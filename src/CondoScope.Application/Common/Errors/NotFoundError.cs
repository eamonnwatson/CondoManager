using FluentResults;

namespace CondoScope.Application.Common.Errors;

public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message)
    {
        WithMetadata("ErrorCode", "APP_NOT_FOUND_ERROR");
        WithMetadata("TimeStamp", DateTime.UtcNow);
    }
}
