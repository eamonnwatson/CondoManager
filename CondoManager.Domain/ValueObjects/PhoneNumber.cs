using CondoManager.Domain.Errors;
using FluentResults;
using System.Text.RegularExpressions;

namespace CondoManager.Domain.ValueObjects;

public sealed partial record PhoneNumber
{
    private PhoneNumber(string value) => Value = value;

    public string Value { get; }

    public static Result<PhoneNumber> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result.Fail(new InvalidPhoneNumber(input));

        string cleaned = CleanPhoneNumber().Replace(input, "");

        if (!PhoneRegex().IsMatch(cleaned))
            return Result.Fail(new InvalidPhoneNumber(input));

        return new PhoneNumber(cleaned);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^\+?[1-9]\d{6,14}$", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"[\s\-\(\)]")]
    private static partial Regex CleanPhoneNumber();
}
