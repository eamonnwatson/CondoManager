using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;

namespace CondoScope.Domain.UnitTests.Entities;

[TestClass]
public class PaymentTests
{
    private static Unit CreateUnit()
    {
        var result = Unit.Create("101", "123 Main St", true, "creator");
        return result.Value;
    }

    [TestMethod]
    public void Create_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var unit = CreateUnit();
        var paymentDate = new DateOnly(2024, 1, 15);
        const decimal amount = 500.75m;
        const PaymentMethod method = PaymentMethod.ETransfer;
        const string reference = "REF-123";
        const string notes = "Some notes";
        const string createdBy = "tester";

        // Act
        var result = Payment.Create(paymentDate, unit, amount, method, reference, notes, createdBy);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
    }

    [TestMethod]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Arrange
        var unit = CreateUnit();
        var paymentDate = new DateOnly(2023, 5, 10);
        const decimal amount = 1200.50m;
        const PaymentMethod method = PaymentMethod.Cheque;
        const string reference = "CHQ-456";
        const string notes = "Payment for May";
        const string createdBy = "admin-user";

        // Act
        var result = Payment.Create(paymentDate, unit, amount, method, reference, notes, createdBy);

        // Assert
        var payment = result.Value;
        Assert.AreEqual(paymentDate, payment.PaymentDate);
        Assert.AreEqual(unit.Id, payment.UnitId);
        Assert.AreEqual(unit, payment.Unit);
        Assert.AreEqual(amount, payment.Amount);
        Assert.AreEqual(method, payment.Method);
        Assert.AreEqual(reference, payment.Reference);
        Assert.AreEqual(notes, payment.Notes);
        Assert.AreEqual(createdBy, payment.CreatedBy);
    }

    [TestMethod]
    public void Create_SetsCreatedAtUtcNow()
    {
        // Arrange
        var unit = CreateUnit();
        var before = DateTime.UtcNow;

        // Act
        var result = Payment.Create(new DateOnly(2024, 3, 1), unit, 100m, PaymentMethod.Cash, null, null, "creator");
        var after = DateTime.UtcNow;

        // Assert
        var payment = result.Value;
        Assert.IsTrue(payment.CreatedAtUtc >= before && payment.CreatedAtUtc <= after);
    }

    [TestMethod]
    public void Create_GeneratesNonEmptyId()
    {
        // Arrange
        var unit = CreateUnit();

        // Act
        var result = Payment.Create(new DateOnly(2024, 2, 2), unit, 250m, PaymentMethod.BankDraft, "R1", "N1", "creator");

        // Assert
        Assert.AreNotEqual(default, result.Value.Id);
    }

    [TestMethod]
    public void Create_WithNullReferenceAndNotes_AllowsNulls()
    {
        // Arrange
        var unit = CreateUnit();

        // Act
        var result = Payment.Create(new DateOnly(2024, 4, 4), unit, 75m, PaymentMethod.Cash, null, null, "creator");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value.Reference);
        Assert.IsNull(result.Value.Notes);
    }

    [TestMethod]
    [DataRow(PaymentMethod.Cheque)]
    [DataRow(PaymentMethod.ETransfer)]
    [DataRow(PaymentMethod.BankDraft)]
    [DataRow(PaymentMethod.Cash)]
    public void Create_WithDifferentPaymentMethods_SetsMethodCorrectly(PaymentMethod method)
    {
        // Arrange
        var unit = CreateUnit();

        // Act
        var result = Payment.Create(new DateOnly(2024, 6, 6), unit, 10m, method, null, null, "creator");

        // Assert
        Assert.AreEqual(method, result.Value.Method);
    }

    [TestMethod]
    public void Create_WithZeroAmount_ReturnsSuccess()
    {
        // Arrange
        var unit = CreateUnit();

        // Act
        var result = Payment.Create(new DateOnly(2024, 7, 7), unit, 0m, PaymentMethod.Cash, null, null, "creator");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0m, result.Value.Amount);
    }

    [TestMethod]
    public void Create_WithNegativeAmount_ReturnsSuccessWithoutValidation()
    {
        // Arrange
        var unit = CreateUnit();

        // Act
        var result = Payment.Create(new DateOnly(2024, 8, 8), unit, -50m, PaymentMethod.Cash, null, null, "creator");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(-50m, result.Value.Amount);
    }
}
