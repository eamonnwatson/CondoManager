using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.FeeCharges;
using CondoScope.Application.FeeCharges.Queries.GetFeeCharges;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using FluentResults;
using Moq;

namespace CondoScope.Application.UnitTests.FeeCharges.Queries.GetFeeCharges;

[TestClass]
public class GetFeeChargesQueryHandlerTests
{
    private readonly Mock<IFeeChargeRepository> feeChargeRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IMapper> mapperMock = new(MockBehavior.Strict);
    private readonly GetFeeChargesQueryHandler handler;

    public GetFeeChargesQueryHandlerTests()
    {
        handler = new GetFeeChargesQueryHandler(feeChargeRepositoryMock.Object, mapperMock.Object);
    }

    [TestMethod]
    public async Task Handle_WhenRepositorySucceeds_ReturnsMappedFeeChargeDtos()
    {
        // Arrange
        var request = new GetFeeChargesQuery();
        var cancellationToken = CancellationToken.None;

        var feeCharge1 = FeeCharge.Create(100m, DateOnly.FromDateTime(DateTime.Today), "Charge1",
            ChargeCategory.CondoFee, ChargeScope.SpecificUnit, Unit.Create("101", "Addr1", true, "creator").Value, "creator").Value;
        var feeCharge2 = FeeCharge.Create(200m, DateOnly.FromDateTime(DateTime.Today), "Charge2",
            ChargeCategory.CondoFee, ChargeScope.SpecificUnit, Unit.Create("102", "Addr2", true, "creator").Value, "creator").Value;
        var feeCharges = new List<FeeCharge> { feeCharge1, feeCharge2 };

        feeChargeRepositoryMock
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<FeeCharge>>(feeCharges));

        var dto1 = new FeeChargeDto("1", DateOnly.FromDateTime(DateTime.Today), "Charge1", 100m,
            ChargeCategory.CondoFee, ChargeScope.SpecificUnit, []);
        var dto2 = new FeeChargeDto("2", DateOnly.FromDateTime(DateTime.Today), "Charge2", 200m,
            ChargeCategory.CondoFee, ChargeScope.SpecificUnit, []);
        var mappedDtos = new List<FeeChargeDto> { dto1, dto2 };

        mapperMock
            .Setup(m => m.Map<FeeChargeDto>((System.Collections.IEnumerable)feeCharges))
            .Returns(mappedDtos);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.AreEqual(new[] { dto1, dto2 }, result.Value.ToList());
        feeChargeRepositoryMock.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<FeeChargeDto>((System.Collections.IEnumerable)feeCharges), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenRepositoryReturnsEmptyList_ReturnsEmptySuccessResult()
    {
        // Arrange
        var request = new GetFeeChargesQuery();
        var cancellationToken = CancellationToken.None;

        var feeCharges = new List<FeeCharge>();
        feeChargeRepositoryMock
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<FeeCharge>>(feeCharges));

        mapperMock
            .Setup(m => m.Map<FeeChargeDto>((System.Collections.IEnumerable)feeCharges))
            .Returns(new List<FeeChargeDto>());

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value.Count());
        feeChargeRepositoryMock.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<FeeChargeDto>((System.Collections.IEnumerable)feeCharges), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WhenRepositoryFails_ReturnsFailureAndDoesNotCallMapper()
    {
        // Arrange
        var request = new GetFeeChargesQuery();
        var cancellationToken = CancellationToken.None;
        const string errorMessage = "Database unavailable";

        feeChargeRepositoryMock
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(Result.Fail<IReadOnlyList<FeeCharge>>(errorMessage));

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.Errors.Any(e => e.Message == errorMessage));
        feeChargeRepositoryMock.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
        mapperMock.Verify(m => m.Map<FeeChargeDto>(It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var request = new GetFeeChargesQuery();
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var feeCharges = new List<FeeCharge>();

        feeChargeRepositoryMock
            .Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<IReadOnlyList<FeeCharge>>(feeCharges));

        mapperMock
            .Setup(m => m.Map<FeeChargeDto>((System.Collections.IEnumerable)feeCharges))
            .Returns(new List<FeeChargeDto>());

        // Act
        await handler.Handle(request, cancellationToken);

        // Assert
        feeChargeRepositoryMock.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }
}
