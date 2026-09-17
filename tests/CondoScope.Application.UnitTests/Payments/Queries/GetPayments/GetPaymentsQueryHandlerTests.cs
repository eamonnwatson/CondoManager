using System.Collections;
using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Payments;
using CondoScope.Application.Payments.Queries.GetPayments;
using CondoScope.Application.Units;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using FluentResults;
using Moq;

namespace CondoScope.Application.UnitTests.Payments.Queries.GetPayments;

[TestClass]
public class GetPaymentsQueryHandlerTests
{
    private readonly Mock<IPaymentRepository> paymentRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IMapper> mapperMock = new(MockBehavior.Strict);

    private GetPaymentsQueryHandler CreateHandler() =>
        new(paymentRepositoryMock.Object, mapperMock.Object);

    [TestMethod]
    public async Task Handle_WhenRepositorySucceeds_ReturnsMappedPaymentDtos()
    {
        // Arrange
        var payment1 = CreatePayment();
        var payment2 = CreatePayment();

        paymentRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Payment>>([payment1, payment2]));

        var dto1 = new PaymentDto("id1", new UnitDto("u1", "101", "Main St", string.Empty, string.Empty), new DateOnly(2024, 1, 1), 10m, PaymentMethod.Cash, "R1", "N1");
        var dto2 = new PaymentDto("id2", new UnitDto("u2", "102", "Main St", string.Empty, string.Empty), new DateOnly(2024, 2, 1), 20m, PaymentMethod.Cheque, "R2", "N2");

        mapperMock
            .Setup(m => m.Map<PaymentDto>(It.IsAny<IEnumerable>()))
            .Returns(new[] { dto1, dto2 });

        var handler = CreateHandler();
        var query = new GetPaymentsQuery();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { dto1, dto2 }, result.Value.ToList());

        paymentRepositoryMock.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<PaymentDto>(It.Is<IEnumerable>(e => e.Cast<Payment>().SequenceEqual(new[] { payment1, payment2 }))), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenRepositorySucceedsWithEmptyList_ReturnsEmptyResult()
    {
        // Arrange
        paymentRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Payment>>([]));

        mapperMock
            .Setup(m => m.Map<PaymentDto>(It.IsAny<IEnumerable>()))
            .Returns([]);

        var handler = CreateHandler();
        var query = new GetPaymentsQuery();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value.Count());

        paymentRepositoryMock.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<PaymentDto>(It.IsAny<IEnumerable>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenRepositoryFails_ReturnsFailureAndDoesNotMap()
    {
        // Arrange
        var error = new Error("payments lookup failed");
        paymentRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IReadOnlyList<Payment>>(error));

        var handler = CreateHandler();
        var query = new GetPaymentsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(result.Errors.ToList(), error);

        paymentRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        mapperMock.Verify(m => m.Map<PaymentDto>(It.IsAny<IEnumerable>()), Times.Never);
    }

    private static Payment CreatePayment()
    {
        var unit = Unit.Create("101", "123 Main St", true, "creator").Value;
        var result = Payment.Create(
            paymentDate: new DateOnly(2024, 1, 1),
            unit: unit,
            amount: 10m,
            method: PaymentMethod.Cash,
            reference: "REF",
            notes: "notes",
            createdBy: "creator");
        return result.Value;
    }
}
