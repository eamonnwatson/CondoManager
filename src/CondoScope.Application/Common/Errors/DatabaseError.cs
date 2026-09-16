using FluentResults;

namespace CondoScope.Application.Common.Errors;

public class DatabaseError : ExceptionalError
{
    public DatabaseError(Exception ex) : base($"A database error occurred.", ex)
    {
        WithMetadata("ErrorCode", "APP_DATABASE_ERROR");
        WithMetadata("TimeStamp", DateTime.UtcNow);
    }

}
