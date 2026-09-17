using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Units.Commands;
using CondoScope.Domain.Entities;
using FluentResults;
using Moq;

namespace CondoScope.Application.UnitTests.Units.Commands;

[TestClass]
public class CreateUnitCommandHandlerTests
{
    private readonly Mock<IUnitsRepository> unitsRepositoryMock = new(MockBehavior.Strict);

    private CreateUnitCommandHandler CreateHandler() => new(unitsRepositoryMock.Object);

    [TestMethod]
    public async Task Handle_WhenAddSucceeds_ReturnsMappedUnitDto()
    {
        // Arrange
        var command = new CreateUnitCommand("101", "123 Main St");

        Unit? capturedUnit = null;
        unitsRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()))
            .Callback<Unit, CancellationToken>((u, _) => capturedUnit = u)
            .ReturnsAsync((Unit u, CancellationToken _) => Result.Ok(u));

        var handler = CreateHandler();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(capturedUnit);
        Assert.AreEqual(capturedUnit!.Id.ToString(), result.Value.UnitId);
        Assert.AreEqual(command.UnitNumber, result.Value.UnitNumber);
        Assert.AreEqual(command.Address, result.Value.Address);
        Assert.AreEqual(string.Empty, result.Value.CurrentOwnerId);
        Assert.AreEqual(string.Empty, result.Value.CurrentOwnerName);
        Assert.AreEqual(command.UnitNumber, capturedUnit.UnitNumber);
        Assert.AreEqual(command.Address, capturedUnit.Address);
        Assert.IsTrue(capturedUnit.IsActive);

        unitsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Unit>(), cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenAddAsyncFails_ReturnsFailure()
    {
        // Arrange
        var command = new CreateUnitCommand("102", "456 Oak St");
        var error = new Error("add failed");

        unitsRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Unit>(error));

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(result.Errors.ToList(), error);

        unitsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
