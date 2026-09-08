using FluentResults;
using System;
using System.Collections.Generic;
using System.Text;

namespace CondoManager.Domain.Errors;

public class InvalidPhoneNumber : Error
{
    public string PhoneNumber { get; }
    public InvalidPhoneNumber(string phoneNumber) : base($"'{phoneNumber}' is not a valid phone number.")
    {
        PhoneNumber = phoneNumber;

        WithMetadata("ErrorCode", "INVALID_PHONE_NUMBER");
        WithMetadata("TimeStamp", DateTime.UtcNow);
    }

}
