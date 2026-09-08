using FluentResults;

namespace CondoScope.Domain.Errors;

public class InvalidEffectiveDate : Error
{
    public DateOnly EffectiveDate { get; }
    public InvalidEffectiveDate(DateOnly effectiveDate) : base($"'{effectiveDate}' must be greater than the last owner's effective date.")
    {
        EffectiveDate = effectiveDate;

        WithMetadata("ErrorCode", "DOM_INVALID_EFFECTIVE_DATE");
        WithMetadata("TimeStamp", DateTime.UtcNow);
    }
}
