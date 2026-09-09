using CondoScope.Domain.ValueObjects;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Text;

namespace CondoScope.Application.Common.Errors;

internal class UnexpectedAppError : ExceptionalError
{
    public UnexpectedAppError(Exception ex) : base($"An unexpected error occurred.", ex)
    {
        WithMetadata("ErrorCode", "APP_UNEXPECTED_ERROR");
        WithMetadata("TimeStamp", DateTime.UtcNow);
    }
}
