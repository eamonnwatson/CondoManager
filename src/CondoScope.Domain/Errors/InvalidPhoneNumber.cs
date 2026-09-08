using FluentResults;

namespace CondoScope.Domain.Errors;

public class InvalidPhoneNumber : Error
{
    public string PhoneNumber { get; }
    public InvalidPhoneNumber(string phoneNumber) : base($"'{phoneNumber}' is not a valid phone number.")
    {
        PhoneNumber = phoneNumber;

        WithMetadata("ErrorCode", "DOM_INVALID_PHONE_NUMBER");
        WithMetadata("TimeStamp", DateTime.UtcNow);
    }

}
