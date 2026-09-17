using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.FeeCharges;
using CondoScope.Application.FeeCharges.Commands.CreateFeeCharge;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using FluentResults;
using Moq;

namespace CondoScope.Application.UnitTests.FeeCharges.Commands.CreateFeeCharge;

[TestClass]
public class CreateFeeChargeCommandHandlerTests
{
    private readonly Mock<IFeeChargeRepository> feeChargeRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitsRepository> unitsRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IMapper> mapperMock = new(MockBehavior.Strict);

    private CreateFeeChargeCommandHandler CreateHandler() =>
        new(feeChargeRepositoryMock.Object, unitsRepositoryMock.Object, mapperMock.Object);

    private static Unit CreateUnit(string unitNumber = "101")
    {
        var result = Unit.Create(unitNumber, "123 Main St", true, "creator");
        return result.Value;
    }

    [TestMethod]
    public async Task Handle_WhenSpecificUnitScopeAndUnitFound_ReturnsMappedDto()
    {
        // Arrange
        var unit = CreateUnit();
        var command = new CreateFeeChargeCommand(
            DueDate: new DateOnly(2024, 3, 1),
            Description: "March Fee",
            Amount: 100m,
            Category: ChargeCategory.CondoFee,
            Scope: ChargeScope.SpecificUnit,
            UnitId: unit.Id.ToString());

        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>([unit]));

        FeeCharge? capturedFeeCharge = null;
        feeChargeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<FeeCharge>(), It.IsAny<CancellationToken>()))
            .Callback<FeeCharge, CancellationToken>((fc, _) => capturedFeeCharge = fc)
            .ReturnsAsync((FeeCharge fc, CancellationToken _) => Result.Ok(fc));

        var expectedDto = new FeeChargeDto(
            Id: "some-id",
            DueDate: command.DueDate,
            Description: command.Description,
            Amount: command.Amount,
            Category: command.Category,
            Scope: command.Scope,
            Units: []);

        mapperMock
            .Setup(m => m.Map<FeeChargeDto>(It.IsAny<object>()))
            .Returns(expectedDto);

        var handler = CreateHandler();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expectedDto, result.Value);
        Assert.IsNotNull(capturedFeeCharge);
        Assert.AreEqual(command.Amount, capturedFeeCharge!.Amount);
        Assert.AreEqual(command.DueDate, capturedFeeCharge.DueDate);
        Assert.AreEqual(command.Description, capturedFeeCharge.Description);
        Assert.AreEqual(command.Category, capturedFeeCharge.Category);
        Assert.AreEqual(command.Scope, capturedFeeCharge.Scope);
        CollectionAssert.Contains(capturedFeeCharge.Units.ToList(), unit);

        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(cancellationToken), Times.Once);
        feeChargeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<FeeCharge>(), cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<FeeChargeDto>(It.IsAny<object>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenAllUnitsScope_IncludesAllUnitsAndReturnsMappedDto()
    {
        // Arrange
        var unit1 = CreateUnit("101");
        var unit2 = CreateUnit("102");
        var command = new CreateFeeChargeCommand(
            DueDate: new DateOnly(2024, 4, 1),
            Description: "April Fee",
            Amount: 200m,
            Category: ChargeCategory.ReserveFee,
            Scope: ChargeScope.AllUnits,
            UnitId: string.Empty);

        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>([unit1, unit2]));

        FeeCharge? capturedFeeCharge = null;
        feeChargeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<FeeCharge>(), It.IsAny<CancellationToken>()))
            .Callback<FeeCharge, CancellationToken>((fc, _) => capturedFeeCharge = fc)
            .ReturnsAsync((FeeCharge fc, CancellationToken _) => Result.Ok(fc));

        var expectedDto = new FeeChargeDto(
            Id: "some-id",
            DueDate: command.DueDate,
            Description: command.Description,
            Amount: command.Amount,
            Category: command.Category,
            Scope: command.Scope,
            Units: []);

        mapperMock
            .Setup(m => m.Map<FeeChargeDto>(It.IsAny<object>()))
            .Returns(expectedDto);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expectedDto, result.Value);
        Assert.IsNotNull(capturedFeeCharge);
        Assert.AreEqual(2, capturedFeeCharge!.Units.Count);
        CollectionAssert.Contains(capturedFeeCharge.Units.ToList(), unit1);
        CollectionAssert.Contains(capturedFeeCharge.Units.ToList(), unit2);

        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()), Times.Once);
        feeChargeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<FeeCharge>(), It.IsAny<CancellationToken>()), Times.Once);
        mapperMock.Verify(m => m.Map<FeeChargeDto>(It.IsAny<object>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenGetAllWithCurrentOwnerFails_ReturnsFailureAndDoesNotAddOrMap()
    {
        // Arrange
        var command = new CreateFeeChargeCommand(
            DueDate: new DateOnly(2024, 3, 1),
            Description: "March Fee",
            Amount: 100m,
            Category: ChargeCategory.CondoFee,
            Scope: ChargeScope.SpecificUnit,
            UnitId: Ulid.NewUlid().ToString());

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
        feeChargeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<FeeCharge>(), It.IsAny<CancellationToken>()), Times.Never);
        mapperMock.Verify(m => m.Map<FeeChargeDto>(It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenSpecificUnitScopeAndUnitIdDoesNotMatchAnyUnit_ReturnsFailureAndDoesNotAddOrMap()
    {
        // Arrange
        var unit = CreateUnit();
        var command = new CreateFeeChargeCommand(
            DueDate: new DateOnly(2024, 3, 1),
            Description: "March Fee",
            Amount: 100m,
            Category: ChargeCategory.CondoFee,
            Scope: ChargeScope.SpecificUnit,
            UnitId: Ulid.NewUlid().ToString());

        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>([unit]));

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);

        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()), Times.Once);
        feeChargeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<FeeCharge>(), It.IsAny<CancellationToken>()), Times.Never);
        mapperMock.Verify(m => m.Map<FeeChargeDto>(It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenAllUnitsScopeAndNoUnitsAvailable_ReturnsFailureAndDoesNotAddOrMap()
    {
        // Arrange
        var command = new CreateFeeChargeCommand(
            DueDate: new DateOnly(2024, 3, 1),
            Description: "March Fee",
            Amount: 100m,
            Category: ChargeCategory.CondoFee,
            Scope: ChargeScope.AllUnits,
            UnitId: string.Empty);

        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>([]));

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);

        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()), Times.Once);
        feeChargeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<FeeCharge>(), It.IsAny<CancellationToken>()), Times.Never);
        mapperMock.Verify(m => m.Map<FeeChargeDto>(It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenAddAsyncFails_ReturnsFailureAndDoesNotMap()
    {
        // Arrange
        var unit = CreateUnit();
        var command = new CreateFeeChargeCommand(
            DueDate: new DateOnly(2024, 3, 1),
            Description: "March Fee",
            Amount: 100m,
            Category: ChargeCategory.CondoFee,
            Scope: ChargeScope.SpecificUnit,
            UnitId: unit.Id.ToString());

        unitsRepositoryMock
            .Setup(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Unit>>([unit]));

        var error = new Error("add failed");
        feeChargeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<FeeCharge>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<FeeCharge>(error));

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(result.Errors.ToList(), error);

        unitsRepositoryMock.Verify(r => r.GetAllWithCurrentOwnerAsync(It.IsAny<CancellationToken>()), Times.Once);
        feeChargeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<FeeCharge>(), It.IsAny<CancellationToken>()), Times.Once);
        mapperMock.Verify(m => m.Map<FeeChargeDto>(It.IsAny<object>()), Times.Never);
    }
}
