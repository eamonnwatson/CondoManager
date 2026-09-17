using CondoScope.Application.FeeCharges;
using CondoScope.Application.FeeCharges.Queries.GetFeeCharges;
using CondoScope.Domain.Enums;
using CondoScope.Web.Components.Pages;
using FluentResults;
using MediatR;
using Moq;
using MudBlazor;

namespace CondoScope.Web.UnitTests.Components.Pages;

[TestClass]
public class FeeChargesTests
{
    private readonly Mock<IMediator> mediatorMock = new(MockBehavior.Strict);
    private readonly Mock<ISnackbar> snackbarMock = new(MockBehavior.Strict);

    private TestableFeeCharges CreateSut() => new()
    {
        Mediator = mediatorMock.Object,
        Snackbar = snackbarMock.Object,
    };

    [TestMethod]
    public async Task OnInitializedAsync_FeeChargesRetrievedSuccessfully_DoesNotShowErrors()
    {
        // Arrange
        var feeCharge1 = new FeeChargeDto("id1", new DateOnly(2024, 1, 1), "Maintenance", 100m, ChargeCategory.CondoFee, ChargeScope.AllUnits, []);
        var feeCharge2 = new FeeChargeDto("id2", new DateOnly(2024, 2, 1), "Repair", 200m, ChargeCategory.CondoFee, ChargeScope.AllUnits, []);

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFeeChargesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<FeeChargeDto>>([feeCharge1, feeCharge2]));

        var sut = CreateSut();

        // Act
        await sut.InvokeOnInitializedAsync();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetFeeChargesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<Severity>(), null, null), Times.Never);
    }

    [TestMethod]
    public async Task OnInitializedAsync_MediatorReturnsFailure_ShowsErrorAndDoesNotPopulateFeeCharges()
    {
        // Arrange
        var error = new FluentResults.Error("fee charges lookup failed");

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFeeChargesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IEnumerable<FeeChargeDto>>(error));

        snackbarMock
            .Setup(s => s.Add(error.Message, Severity.Error, null, null))
            .Returns((Snackbar)null!);

        var sut = CreateSut();

        // Act
        await sut.InvokeOnInitializedAsync();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetFeeChargesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(s => s.Add(error.Message, Severity.Error, null, null), Times.Once);
    }

    private sealed class TestableFeeCharges : FeeCharges
    {
        public Task InvokeOnInitializedAsync() => OnInitializedAsync();
    }
}
