using CondoScope.Domain.ValueObjects;
using FluentResults;

namespace CondoScope.Domain.UnitTests.ValueObjects;

[TestClass]
public class EmailTests
{
    [TestMethod]
    public void Create_NullEmail_ReturnsFailure()
    {
        Result<Email> result = Email.Create(null!);

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_EmptyEmail_ReturnsFailure()
    {
        Result<Email> result = Email.Create(string.Empty);

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_WhitespaceEmail_ReturnsFailure()
    {
        Result<Email> result = Email.Create("   ");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_InvalidFormat_ReturnsFailure()
    {
        Result<Email> result = Email.Create("not-an-email");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_MissingDomain_ReturnsFailure()
    {
        Result<Email> result = Email.Create("user@");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_MissingAtSign_ReturnsFailure()
    {
        Result<Email> result = Email.Create("user.example.com");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_ValidEmail_ReturnsSuccess()
    {
        Result<Email> result = Email.Create("user@example.com");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("user@example.com", result.Value.Value);
    }

    [TestMethod]
    public void Create_ValidEmailWithWhitespace_TrimsAndSucceeds()
    {
        Result<Email> result = Email.Create("  user@example.com  ");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("user@example.com", result.Value.Value);
    }

    [TestMethod]
    public void Create_ValidEmailWithUppercase_ConvertsToLowercase()
    {
        Result<Email> result = Email.Create("User@Example.COM");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("user@example.com", result.Value.Value);
    }

    [TestMethod]
    public void ToString_ValidEmail_ReturnsValue()
    {
        Result<Email> result = Email.Create("user@example.com");

        string toString = result.Value.ToString();

        Assert.AreEqual("user@example.com", toString);
    }
}
