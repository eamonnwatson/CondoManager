using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Owners;
using CondoScope.Application.Owners.Commands.CreateOwner;
using CondoScope.Domain.Entities;
using FluentResults;
using Moq;

namespace CondoScope.Application.UnitTests.Owners.Commands.CreateOwner;

[TestClass]
public class CreateOwnerCommandHandlerTests
{
    private readonly Mock<IOwnersRepository> _ownersRepositoryMock = new();
    private readonly Mock<IUnitsRepository> _unitsRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private CreateOwnerCommandHandler CreateSut() =>
        new(_ownersRepositoryMock.Object, _unitsRepositoryMock.Object, _mapperMock.Object);

    private static Unit CreateUnit(string unitNumber = "101") =>
        Unit.Create(unitNumber, "123 Main St", true, "tester").Value;

    [TestMethod]
    public async Task Handle_WithValidDataAndNoUnit_ReturnsSuccessAndAddsOwner()
    {
        // Arrange
        var request = new CreateOwnerCommand("John Doe", "john@example.com", "+15551234567", null, null);
        var expectedDto = new OwnerDto("id", "unit", "John Doe", "addr", "john@example.com", "+15551234567");

        _ownersRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Owner owner, CancellationToken _) => Result.Ok(owner));

        _mapperMock
            .Setup(m => m.Map<OwnerDto>(It.IsAny<object>()))
            .Returns(expectedDto);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expectedDto, result.Value);
        _ownersRepositoryMock.Verify(r => r.AddAsync(It.Is<Owner>(o => o.Name == "John Doe"), It.IsAny<CancellationToken>()), Times.Once);
        _unitsRepositoryMock.Verify(r => r.GetByIdWithDetailsAsync(It.IsAny<Ulid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithInvalidEmail_ReturnsFailureAndDoesNotAddOwner()
    {
        // Arrange
        var request = new CreateOwnerCommand("John Doe", "not-an-email", null, null, null);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        _ownersRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(m => m.Map<OwnerDto>(It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithInvalidPhoneNumber_ReturnsFailureAndDoesNotAddOwner()
    {
        // Arrange
        var request = new CreateOwnerCommand("John Doe", null, "abc", null, null);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        _ownersRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithNonParsableUnitId_ReturnsFailureAndDoesNotCallUnitsRepository()
    {
        // Arrange
        var request = new CreateOwnerCommand("John Doe", null, null, "not-a-ulid", null);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("Invalid Unit Id.", result.Errors[0].Message);
        _unitsRepositoryMock.Verify(r => r.GetByIdWithDetailsAsync(It.IsAny<Ulid>(), It.IsAny<CancellationToken>()), Times.Never);
        _ownersRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithValidUnitIdAndUnitFound_AssignsOwnerAndAddsOwner()
    {
        // Arrange
        var unitId = Ulid.NewUlid();
        var request = new CreateOwnerCommand("John Doe", null, null, unitId.ToString(), new DateOnly(2024, 1, 1));
        var unit = CreateUnit();
        var expectedDto = new OwnerDto("id", "101", "John Doe", "addr", string.Empty, string.Empty);

        _unitsRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(unit));

        _ownersRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Owner owner, CancellationToken _) => Result.Ok(owner));

        _mapperMock
            .Setup(m => m.Map<OwnerDto>(It.IsAny<object>()))
            .Returns(expectedDto);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expectedDto, result.Value);
        Assert.HasCount(1, unit.UnitOwners);
        _unitsRepositoryMock.Verify(r => r.GetByIdWithDetailsAsync(unitId, It.IsAny<CancellationToken>()), Times.Once);
        _ownersRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WithUnitIdNotFound_ReturnsFailureAndDoesNotAddOwner()
    {
        // Arrange
        var unitId = Ulid.NewUlid();
        var request = new CreateOwnerCommand("John Doe", null, null, unitId.ToString(), null);

        _unitsRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Unit>("Unit not found."));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        _ownersRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithUnitAssignmentFailure_ReturnsFailureAndDoesNotAddOwner()
    {
        // Arrange
        var unitId = Ulid.NewUlid();
        var request = new CreateOwnerCommand("John Doe", null, null, unitId.ToString(), new DateOnly(2024, 1, 1));
        var unit = CreateUnit();
        var otherOwner = Owner.Create("Existing Owner", null, null, "tester").Value;
        unit.AssignOwner(otherOwner, new DateOnly(2024, 6, 1), "tester");

        _unitsRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(unit));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        _ownersRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithWhitespaceUnitId_TreatsAsNoUnitAndDoesNotCallUnitsRepository()
    {
        // Arrange
        var request = new CreateOwnerCommand("John Doe", null, null, "   ", null);
        var expectedDto = new OwnerDto("id", string.Empty, "John Doe", string.Empty, string.Empty, string.Empty);

        _ownersRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Owner owner, CancellationToken _) => Result.Ok(owner));

        _mapperMock
            .Setup(m => m.Map<OwnerDto>(It.IsAny<object>()))
            .Returns(expectedDto);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        _unitsRepositoryMock.Verify(r => r.GetByIdWithDetailsAsync(It.IsAny<Ulid>(), It.IsAny<CancellationToken>()), Times.Never);
        _ownersRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenOwnersRepositoryAddFails_ReturnsFailureAndDoesNotMap()
    {
        // Arrange
        var request = new CreateOwnerCommand("John Doe", null, null, null, null);

        _ownersRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Owner>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Owner>("Database error."));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        _mapperMock.Verify(m => m.Map<OwnerDto>(It.IsAny<object>()), Times.Never);
    }
}
