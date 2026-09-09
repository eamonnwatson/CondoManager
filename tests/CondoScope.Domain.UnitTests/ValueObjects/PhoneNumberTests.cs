using CondoScope.Domain.Errors;
using CondoScope.Domain.ValueObjects;

namespace CondoScope.Domain.UnitTests.ValueObjects;

[TestClass]
public class PhoneNumberTests
{
    [TestMethod]
    public void Create_WithNullInput_ReturnsFailure()
    {
        var result = PhoneNumber.Create(null!);

        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<InvalidPhoneNumber>(result.Errors[0]);
    }

    [TestMethod]
    public void Create_WithEmptyInput_ReturnsFailure()
    {
        var result = PhoneNumber.Create(string.Empty);

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_WithWhitespaceInput_ReturnsFailure()
    {
        var result = PhoneNumber.Create("   ");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_WithInvalidCharacters_ReturnsFailure()
    {
        var result = PhoneNumber.Create("abcdefg");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_WithLeadingZero_ReturnsFailure()
    {
        var result = PhoneNumber.Create("01234567");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_WithTooShortNumber_ReturnsFailure()
    {
        var result = PhoneNumber.Create("123456");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_WithTooLongNumber_ReturnsFailure()
    {
        var result = PhoneNumber.Create("1234567890123456");

        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_WithValidNumberNoPlus_ReturnsSuccess()
    {
        var result = PhoneNumber.Create("1234567");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("1234567", result.Value.Value);
    }

    [TestMethod]
    public void Create_WithValidNumberWithPlus_ReturnsSuccess()
    {
        var result = PhoneNumber.Create("+1234567890");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("+1234567890", result.Value.Value);
    }

    [TestMethod]
    public void Create_WithSpacesDashesParens_CleansAndReturnsSuccess()
    {
        var result = PhoneNumber.Create("+1 (234) 567-8901");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("+12345678901", result.Value.Value);
    }

    [TestMethod]
    public void ToString_ReturnsValue()
    {
        var result = PhoneNumber.Create("+1234567890");

        Assert.AreEqual("+1234567890", result.Value.ToString());
    }
}
