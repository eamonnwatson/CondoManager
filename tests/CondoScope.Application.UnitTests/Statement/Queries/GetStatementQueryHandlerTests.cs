using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Statement;
using CondoScope.Application.Statement.Queries;
using CondoScope.Application.Ledger;
using CondoScope.Domain.Entities;
using FluentResults;
using Moq;

namespace CondoScope.Application.UnitTests.Statement.Queries;

[TestClass]
public class GetStatementQueryHandlerTests
{
    private readonly Mock<IUnitsRepository> _unitsRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private GetStatementQueryHandler CreateSut() =>
        new(_unitsRepositoryMock.Object, _mapperMock.Object);

    private static Unit CreateUnit(string unitNumber = "101") =>
        Unit.Create(unitNumber, "123 Main St", true, "tester").Value;

    [TestMethod]
    public async Task Handle_WhenUnitFound_ReturnsSuccessWithMappedDto()
    {
        // Arrange
        var unitId = Ulid.NewUlid();
        var unit = CreateUnit();
        var request = new GetStatementQuery(unitId.ToString());
        var expectedDto = new StatementDto("101", "John Doe", "addr", "email", "phone", 100m, AccountStatus.PaidInFull, []);

        _unitsRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(unit));

        _mapperMock
            .Setup(m => m.Map<StatementDto>(unit))
            .Returns(expectedDto);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expectedDto, result.Value);
        _unitsRepositoryMock.Verify(r => r.GetByIdWithDetailsAsync(unitId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<StatementDto>(unit), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenUnitNotFound_ReturnsFailureAndDoesNotMap()
    {
        // Arrange
        var unitId = Ulid.NewUlid();
        var request = new GetStatementQuery(unitId.ToString());

        _unitsRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Unit>("Unit not found."));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Unit not found.", result.Errors[0].Message);
        _unitsRepositoryMock.Verify(r => r.GetByIdWithDetailsAsync(unitId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<StatementDto>(It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithInvalidUnitId_ThrowsArgumentException()
    {
        // Arrange
        var request = new GetStatementQuery("not-a-valid-ulid");
        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => sut.Handle(request, CancellationToken.None));
        _unitsRepositoryMock.Verify(r => r.GetByIdWithDetailsAsync(It.IsAny<Ulid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
