using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Units;
using CondoScope.Application.Units.Queries.GetUnits;
using CondoScope.Domain.Entities;
using FluentResults;
using Moq;

namespace CondoScope.Application.UnitTests.Units.Queries.GetUnits;

[TestClass]
public class GetUnitsQueryHandlerTests
{
    private readonly Mock<IUnitsRepository> unitsRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IMapper> mapperMock = new(MockBehavior.Strict);
    private readonly GetUnitsQueryHandler handler;

    public GetUnitsQueryHandlerTests()
    {
        handler = new GetUnitsQueryHandler(unitsRepositoryMock.Object, mapperMock.Object);
    }

    [TestMethod]
    public async Task Handle_WhenOnlyUnitsWithNoOwnersIsFalse_CallsGetAllWithCurrentOwnerAndReturnsMappedDtos()
    {
        // Arrange
        var request = new GetUnitsQuery(OnlyUnitsWithNoOwners: false);
        var cancellationToken = CancellationToken.None;

        var unit1Result = Unit.Create("101", "123 Main St", true, "creator");
        var unit2Result = Unit.Create("102", "125 Main St", true, "creator");
        var units = new List<Unit> { unit1Result.Value, unit2Result.Value };

        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>(units));

        var dto1 = new UnitDto("id1", "101", "123 Main St", "owner1", "Owner One");
        var dto2 = new UnitDto("id2", "102", "125 Main St", "owner2", "Owner Two");
        var dtos = new List<UnitDto> { dto1, dto2 };

        mapperMock
            .Setup(m => m.Map<UnitDto>((System.Collections.IEnumerable)units))
            .Returns(dtos);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { dto1, dto2 }, result.Value.ToList());
        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(cancellationToken), Times.Once);
        unitsRepositoryMock.Verify(r => r.GetUnitsWithNoOwnersAsync(It.IsAny<CancellationToken>()), Times.Never);
        mapperMock.Verify(m => m.Map<UnitDto>((System.Collections.IEnumerable)units), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenOnlyUnitsWithNoOwnersIsTrue_CallsGetUnitsWithNoOwnersAndReturnsMappedDtos()
    {
        // Arrange
        var request = new GetUnitsQuery(OnlyUnitsWithNoOwners: true);
        var cancellationToken = CancellationToken.None;

        var unitResult = Unit.Create("201", "1 Elm St", true, "creator");
        var units = new List<Unit> { unitResult.Value };

        unitsRepositoryMock
            .Setup(r => r.GetUnitsWithNoOwnersAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>(units));

        var dto = new UnitDto("id1", "201", "1 Elm St", string.Empty, string.Empty);
        var dtos = new List<UnitDto> { dto };

        mapperMock
            .Setup(m => m.Map<UnitDto>((System.Collections.IEnumerable)units))
            .Returns(dtos);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { dto }, result.Value.ToList());
        unitsRepositoryMock.Verify(r => r.GetUnitsWithNoOwnersAsync(cancellationToken), Times.Once);
        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()), Times.Never);
        mapperMock.Verify(m => m.Map<UnitDto>((System.Collections.IEnumerable)units), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenOnlyUnitsWithNoOwnersFalseAndRepositoryReturnsEmptyList_ReturnsEmptySuccessResult()
    {
        // Arrange
        var request = new GetUnitsQuery(OnlyUnitsWithNoOwners: false);
        var cancellationToken = CancellationToken.None;

        var emptyUnits = new List<Unit>();
        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>(emptyUnits));

        mapperMock
            .Setup(m => m.Map<UnitDto>((System.Collections.IEnumerable)emptyUnits))
            .Returns(new List<UnitDto>());

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value.Count());
        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<UnitDto>((System.Collections.IEnumerable)emptyUnits), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenOnlyUnitsWithNoOwnersFalseAndRepositoryFails_ReturnsFailureAndDoesNotCallMapper()
    {
        // Arrange
        var request = new GetUnitsQuery(OnlyUnitsWithNoOwners: false);
        var cancellationToken = CancellationToken.None;
        const string errorMessage = "Database unavailable";

        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(cancellationToken))
            .ReturnsAsync(Result.Fail<IReadOnlyList<Unit>>(errorMessage));

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors.Any(e => e.Message == errorMessage));
        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<UnitDto>(It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenOnlyUnitsWithNoOwnersTrueAndRepositoryFails_ReturnsFailureAndDoesNotCallMapper()
    {
        // Arrange
        var request = new GetUnitsQuery(OnlyUnitsWithNoOwners: true);
        var cancellationToken = CancellationToken.None;
        const string errorMessage = "No owners lookup failed";

        unitsRepositoryMock
            .Setup(r => r.GetUnitsWithNoOwnersAsync(cancellationToken))
            .ReturnsAsync(Result.Fail<IReadOnlyList<Unit>>(errorMessage));

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors.Any(e => e.Message == errorMessage));
        unitsRepositoryMock.Verify(r => r.GetUnitsWithNoOwnersAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<UnitDto>(It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var request = new GetUnitsQuery(OnlyUnitsWithNoOwners: false);
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        var emptyUnits = new List<Unit>();
        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>(emptyUnits));

        mapperMock
            .Setup(m => m.Map<UnitDto>((System.Collections.IEnumerable)emptyUnits))
            .Returns(new List<UnitDto>());

        // Act
        await handler.Handle(request, cancellationToken);

        // Assert
        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(cancellationToken), Times.Once);
    }
}
