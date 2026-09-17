using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Owners;
using CondoScope.Application.Owners.Queries.GetOwners;
using CondoScope.Domain.Entities;
using CondoScope.Domain.ValueObjects;
using FluentResults;
using Moq;

namespace CondoScope.Application.UnitTests.Owners.Queries.GetOwners;

[TestClass]
public class GetOwnersQueryHandlerTests
{
    private readonly Mock<IOwnersRepository> ownersRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IMapper> mapperMock = new(MockBehavior.Strict);
    private readonly GetOwnersQueryHandler handler;

    public GetOwnersQueryHandlerTests()
    {
        handler = new GetOwnersQueryHandler(ownersRepositoryMock.Object, mapperMock.Object);
    }

    [TestMethod]
    public async Task Handle_WhenRepositorySucceeds_ReturnsMappedOwnerDtos()
    {
        // Arrange
        var request = new GetOwnersQuery();
        var cancellationToken = CancellationToken.None;

        var owner1 = Owner.Create("Owner1", Email.Create("owner1@example.com").Value, PhoneNumber.Create("1234567890").Value, "creator").Value;
        var owner2 = Owner.Create("Owner2", Email.Create("owner2@example.com").Value, PhoneNumber.Create("1987654321").Value, "creator").Value;
        var owners = new List<Owner> { owner1, owner2 };

        ownersRepositoryMock
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Owner>>(owners));

        var dto1 = new OwnerDto("1", "101", "Owner1", "Addr1", "owner1@example.com", "1234567890");
        var dto2 = new OwnerDto("2", "102", "Owner2", "Addr2", "owner2@example.com", "1987654321");
        var mappedDtos = new List<OwnerDto> { dto1, dto2 };

        mapperMock
            .Setup(m => m.Map<OwnerDto>((System.Collections.IEnumerable)owners))
            .Returns(mappedDtos);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { dto1, dto2 }, result.Value.ToList());
        ownersRepositoryMock.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<OwnerDto>((System.Collections.IEnumerable)owners), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenRepositoryReturnsEmptyList_ReturnsEmptySuccessResult()
    {
        // Arrange
        var request = new GetOwnersQuery();
        var cancellationToken = CancellationToken.None;

        var owners = new List<Owner>();
        ownersRepositoryMock
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Owner>>(owners));

        mapperMock
            .Setup(m => m.Map<OwnerDto>((System.Collections.IEnumerable)owners))
            .Returns(new List<OwnerDto>());

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value.Count());
        ownersRepositoryMock.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<OwnerDto>((System.Collections.IEnumerable)owners), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenRepositoryFails_ReturnsFailureAndDoesNotCallMapper()
    {
        // Arrange
        var request = new GetOwnersQuery();
        var cancellationToken = CancellationToken.None;
        const string errorMessage = "Database unavailable";

        ownersRepositoryMock
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(Result.Fail<IReadOnlyList<Owner>>(errorMessage));

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors.Any(e => e.Message == errorMessage));
        ownersRepositoryMock.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<OwnerDto>(It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var request = new GetOwnersQuery();
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var owners = new List<Owner>();

        ownersRepositoryMock
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Owner>>(owners));

        mapperMock
            .Setup(m => m.Map<OwnerDto>((System.Collections.IEnumerable)owners))
            .Returns(new List<OwnerDto>());

        // Act
        await handler.Handle(request, cancellationToken);

        // Assert
        ownersRepositoryMock.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }
}
