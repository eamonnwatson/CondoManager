using CondoManager.Domain.Errors;
using FluentResults;
using System.Text.RegularExpressions;

namespace CondoManager.Domain.ValueObjects;

public sealed partial record Email
{
    private Email(string value) => Value = value;

    public string Value { get; }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Fail(new InvalidEmailError(email));

        string trimmedEmail = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(trimmedEmail))
            return Result.Fail(new InvalidEmailError(email));

        return new Email(trimmedEmail);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-CA")]
    private static partial Regex EmailRegex();
}
