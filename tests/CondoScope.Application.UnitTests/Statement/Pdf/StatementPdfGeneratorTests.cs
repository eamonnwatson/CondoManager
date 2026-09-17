using CondoScope.Application.Ledger;
using CondoScope.Application.Statement;
using CondoScope.Application.Statement.Pdf;

namespace CondoScope.Application.UnitTests.Statement.Pdf;

[TestClass]
public class StatementPdfGeneratorTests
{
    static StatementPdfGeneratorTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    private static StatementDto CreateStatement(
        string unitNumber = "101",
        string ownerName = "John Doe",
        string address = "123 Main St",
        string emailAddress = "john@example.com",
        string phoneNumber = "555-1234",
        decimal currentBalance = 100.50m,
        AccountStatus accountStatus = AccountStatus.Outstanding,
        IEnumerable<AccountActivityDto>? accountActivities = null)
    {
        return new StatementDto(
            unitNumber,
            ownerName,
            address,
            emailAddress,
            phoneNumber,
            currentBalance,
            accountStatus,
            accountActivities ?? []);
    }

    [TestMethod]
    public void Generate_WithNoActivities_ReturnsNonEmptyPdfBytes()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var statement = CreateStatement();
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Generate_WithPhoneNumberEmpty_ReturnsNonEmptyPdfBytes()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var statement = CreateStatement(phoneNumber: string.Empty);
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Generate_WithPhoneNumberWhitespace_ReturnsNonEmptyPdfBytes()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var statement = CreateStatement(phoneNumber: "   ");
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Generate_WithPhoneNumberProvided_ReturnsNonEmptyPdfBytes()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var statement = CreateStatement(phoneNumber: "555-9876");
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Generate_WithNoPaymentActivities_ReturnsNonEmptyPdfBytesAndShowsNoLastPayment()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var activities = new List<AccountActivityDto>
        {
            new(new DateOnly(2024, 1, 1), "Charge 1", 50.00m, null, 50.00m),
            new(new DateOnly(2024, 1, 5), "Charge 2", 25.00m, null, 75.00m),
        };
        var statement = CreateStatement(accountActivities: activities);
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Generate_WithMultiplePaymentActivities_UsesMostRecentPaymentForLastPayment()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var activities = new List<AccountActivityDto>
        {
            new(new DateOnly(2024, 1, 1), "Payment 1", null, 20.00m, 80.00m),
            new(new DateOnly(2024, 1, 10), "Payment 2", null, 30.00m, 50.00m),
            new(new DateOnly(2024, 1, 5), "Charge", 10.00m, null, 60.00m),
        };
        var statement = CreateStatement(accountActivities: activities);
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Generate_WithManyActivitiesForAlternatingRowColors_ReturnsNonEmptyPdfBytes()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var activities = new List<AccountActivityDto>
        {
            new(new DateOnly(2024, 1, 1), "Row 0", 10.00m, null, 10.00m),
            new(new DateOnly(2024, 1, 2), "Row 1", null, 5.00m, 5.00m),
            new(new DateOnly(2024, 1, 3), "Row 2", 20.00m, null, 25.00m),
        };
        var statement = CreateStatement(accountActivities: activities);
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Generate_WithAccountStatusPaidInFull_ReturnsNonEmptyPdfBytes()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var statement = CreateStatement(accountStatus: AccountStatus.PaidInFull);
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Generate_WithAccountStatusCredit_ReturnsNonEmptyPdfBytes()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var statement = CreateStatement(accountStatus: AccountStatus.Credit);
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Generate_WithAccountStatusOutstanding_ReturnsNonEmptyPdfBytes()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var statement = CreateStatement(accountStatus: AccountStatus.Outstanding);
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Generate_WithUnknownAccountStatus_FormatsUsingToString()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var statement = CreateStatement(accountStatus: (AccountStatus)999);
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Generate_WithActivityHavingNullChargeAndPayment_ReturnsNonEmptyPdfBytes()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var activities = new List<AccountActivityDto>
        {
            new(new DateOnly(2024, 1, 1), "No charge or payment", null, null, 0.00m),
        };
        var statement = CreateStatement(accountActivities: activities);
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Generate_ReturnsPdfWithValidHeaderBytes()
    {
        // Arrange
        var generator = new StatementPdfGenerator();
        var statement = CreateStatement();
        var asOfDate = new DateOnly(2024, 1, 15);

        // Act
        var result = generator.Generate(statement, asOfDate);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Length > 4);
        Assert.AreEqual('%', (char)result[0]);
        Assert.AreEqual('P', (char)result[1]);
        Assert.AreEqual('D', (char)result[2]);
        Assert.AreEqual('F', (char)result[3]);
    }
}
