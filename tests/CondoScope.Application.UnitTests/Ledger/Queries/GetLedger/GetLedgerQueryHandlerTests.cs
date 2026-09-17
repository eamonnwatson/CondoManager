using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Ledger;
using CondoScope.Application.Ledger.Queries.GetLedger;
using CondoScope.Domain.Entities;
using FluentResults;
using Moq;

namespace CondoScope.Application.UnitTests.Ledger.Queries.GetLedger;

[TestClass]
public class GetLedgerQueryHandlerTests
{
    private readonly Mock<IUnitsRepository> unitsRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IMapper> mapperMock = new(MockBehavior.Strict);
    private readonly GetLedgerQueryHandler handler;

    public GetLedgerQueryHandlerTests()
    {
        handler = new GetLedgerQueryHandler(unitsRepositoryMock.Object, mapperMock.Object);
    }

    [TestMethod]
    public async Task Handle_WhenRepositorySucceeds_ReturnsMappedLedgerDTOs()
    {
        // Arrange
        var request = new GetLedgerQuery();
        var cancellationToken = CancellationToken.None;

        var unit1Result = Unit.Create("101", "123 Main St", true, "creator");
        var unit2Result = Unit.Create("102", "125 Main St", true, "creator");
        var units = new List<Unit> { unit1Result.Value, unit2Result.Value };

        unitsRepositoryMock
            .Setup(r => r.GetAllWithDetailsAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>(units));

        var dto1 = new LedgerDTO("101", "Owner1", AccountStatus.PaidInFull, 0m, 0m, 0m, 0m, 0m);
        var dto2 = new LedgerDTO("102", "Owner2", AccountStatus.PaidInFull, 0m, 0m, 0m, 0m, 0m);
        var mappedDtos = new List<LedgerDTO> { dto1, dto2 };

        mapperMock
            .Setup(m => m.Map<LedgerDTO>((System.Collections.IEnumerable)units))
            .Returns(mappedDtos);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { dto1, dto2 }, result.Value.ToList());
        unitsRepositoryMock.Verify(r => r.GetAllWithDetailsAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<LedgerDTO>((System.Collections.IEnumerable)units), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenRepositoryReturnsEmptyList_ReturnsEmptySuccessResult()
    {
        // Arrange
        var request = new GetLedgerQuery();
        var cancellationToken = CancellationToken.None;

        var units = new List<Unit>();
        unitsRepositoryMock
            .Setup(r => r.GetAllWithDetailsAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>(units));

        mapperMock
            .Setup(m => m.Map<LedgerDTO>((System.Collections.IEnumerable)units))
            .Returns(new List<LedgerDTO>());

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value.Count());
        unitsRepositoryMock.Verify(r => r.GetAllWithDetailsAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<LedgerDTO>((System.Collections.IEnumerable)units), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenRepositoryFails_ReturnsFailureAndDoesNotCallMapper()
    {
        // Arrange
        var request = new GetLedgerQuery();
        var cancellationToken = CancellationToken.None;
        const string errorMessage = "Database unavailable";

        unitsRepositoryMock
            .Setup(r => r.GetAllWithDetailsAsync(cancellationToken))
            .ReturnsAsync(Result.Fail<IReadOnlyList<Unit>>(errorMessage));

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors.Any(e => e.Message == errorMessage));
        unitsRepositoryMock.Verify(r => r.GetAllWithDetailsAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<LedgerDTO>(It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var request = new GetLedgerQuery();
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var units = new List<Unit>();

        unitsRepositoryMock
            .Setup(r => r.GetAllWithDetailsAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>(units));

        mapperMock
            .Setup(m => m.Map<LedgerDTO>((System.Collections.IEnumerable)units))
            .Returns(new List<LedgerDTO>());

        // Act
        await handler.Handle(request, cancellationToken);

        // Assert
        unitsRepositoryMock.Verify(r => r.GetAllWithDetailsAsync(cancellationToken), Times.Once);
    }
}
