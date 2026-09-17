using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Payments;
using CondoScope.Application.Payments.Commands.CreatePayment;
using CondoScope.Application.Units;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using FluentResults;
using Moq;

namespace CondoScope.Application.UnitTests.Payments.Commands.CreatePayment;

[TestClass]
public class CreatePaymentCommandHandlerTests
{
    private readonly Mock<IPaymentRepository> paymentRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitsRepository> unitsRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IMapper> mapperMock = new(MockBehavior.Strict);

    private CreatePaymentCommandHandler CreateHandler() =>
        new(paymentRepositoryMock.Object, unitsRepositoryMock.Object, mapperMock.Object);

    private static Unit CreateUnit()
    {
        var result = Unit.Create("101", "123 Main St", true, "creator");
        return result.Value;
    }

    [TestMethod]
    public async Task Handle_WhenUnitFoundAndPaymentCreated_ReturnsMappedPaymentDto()
    {
        // Arrange
        var unit = CreateUnit();
        var command = new CreatePaymentCommand(
            PaymentDate: new DateOnly(2024, 1, 15),
            UnitId: unit.Id.ToString(),
            Amount: 100.50m,
            PaymentMethod: PaymentMethod.Cash,
            ReferenceNumber: "REF-1",
            Notes: "Some notes");

        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>([unit]));

        Payment? capturedPayment = null;
        paymentRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((p, _) => capturedPayment = p)
            .ReturnsAsync((Payment p, CancellationToken _) => Result.Ok(p));

        var expectedDto = new PaymentDto(
            Id: "some-id",
            Unit: new UnitDto(unit.Id.ToString(), "101", "123 Main St", string.Empty, string.Empty),
            PaymentDate: command.PaymentDate,
            Amount: command.Amount,
            PaymentMethod: command.PaymentMethod,
            Reference: command.ReferenceNumber!,
            Notes: command.Notes!);

        mapperMock
            .Setup(m => m.Map<PaymentDto>(It.IsAny<object>()))
            .Returns(expectedDto);

        var handler = CreateHandler();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expectedDto, result.Value);
        Assert.IsNotNull(capturedPayment);
        Assert.AreEqual(unit.Id, capturedPayment!.UnitId);
        Assert.AreEqual(command.Amount, capturedPayment.Amount);
        Assert.AreEqual(command.PaymentMethod, capturedPayment.Method);
        Assert.AreEqual(command.ReferenceNumber, capturedPayment.Reference);
        Assert.AreEqual(command.Notes, capturedPayment.Notes);
        Assert.AreEqual(command.PaymentDate, capturedPayment.PaymentDate);

        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(cancellationToken), Times.Once);
        paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<PaymentDto>(It.IsAny<object>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenGetAllWithCurrentOwnerFails_ReturnsFailureAndDoesNotAddPaymentOrMap()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            PaymentDate: new DateOnly(2024, 1, 15),
            UnitId: Ulid.NewUlid().ToString(),
            Amount: 50m,
            PaymentMethod: PaymentMethod.Cheque,
            ReferenceNumber: "REF-2",
            Notes: null);

        var error = new Error("units lookup failed");
        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IReadOnlyList<Unit>>(error));

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(result.Errors.ToList(), error);

        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()), Times.Once);
        paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
        mapperMock.Verify(m => m.Map<PaymentDto>(It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenUnitIdDoesNotMatchAnyUnit_ThrowsInvalidOperationException()
    {
        // Arrange
        var unit = CreateUnit();
        var command = new CreatePaymentCommand(
            PaymentDate: new DateOnly(2024, 1, 15),
            UnitId: Ulid.NewUlid().ToString(),
            Amount: 25m,
            PaymentMethod: PaymentMethod.BankDraft,
            ReferenceNumber: null,
            Notes: null);

        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>([unit]));

        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()), Times.Once);
        paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
        mapperMock.Verify(m => m.Map<PaymentDto>(It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenAddAsyncFails_ReturnsFailureAndDoesNotMap()
    {
        // Arrange
        var unit = CreateUnit();
        var command = new CreatePaymentCommand(
            PaymentDate: new DateOnly(2024, 1, 15),
            UnitId: unit.Id.ToString(),
            Amount: 75m,
            PaymentMethod: PaymentMethod.ETransfer,
            ReferenceNumber: "REF-3",
            Notes: "notes");

        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>([unit]));

        var error = new Error("add failed");
        paymentRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Payment>(error));

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(result.Errors.ToList(), error);

        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()), Times.Once);
        paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        mapperMock.Verify(m => m.Map<PaymentDto>(It.IsAny<object>()), Times.Never);
    }
}
