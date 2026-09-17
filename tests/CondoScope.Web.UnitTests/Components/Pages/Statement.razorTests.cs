using CondoScope.Application.Units;
using CondoScope.Application.Units.Queries.GetUnits;
using CondoScope.Web.Components.Pages;
using FluentResults;
using MediatR;
using Moq;
using MudBlazor;

namespace CondoScope.Web.UnitTests.Components.Pages;

[TestClass]
public class StatementTests
{
    private readonly Mock<IMediator> mediatorMock = new(MockBehavior.Strict);
    private readonly Mock<ISnackbar> snackbarMock = new(MockBehavior.Strict);

    private TestableStatement CreateSut() => new()
    {
        Mediator = mediatorMock.Object,
        Snackbar = snackbarMock.Object,
    };

    [TestMethod]
    public async Task OnInitializedAsync_UnitsRetrievedSuccessfully_DoesNotShowErrors()
    {
        // Arrange
        var unit1 = new UnitDto("id1", "101", "123 Main St", "owner1", "Owner One");
        var unit2 = new UnitDto("id2", "102", "125 Main St", "owner2", "Owner Two");

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUnitsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<UnitDto>>([unit1, unit2]));

        var sut = CreateSut();

        // Act
        await sut.InvokeOnInitializedAsync();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetUnitsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<Severity>(), null, null), Times.Never);
    }

    [TestMethod]
    public async Task OnInitializedAsync_MediatorReturnsFailure_ShowsErrorAndDoesNotPopulateUnits()
    {
        // Arrange
        var error = new FluentResults.Error("units lookup failed");

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUnitsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IEnumerable<UnitDto>>(error));

        snackbarMock
            .Setup(s => s.Add(error.Message, Severity.Error, null, null))
            .Returns((Snackbar)null!);

        var sut = CreateSut();

        // Act
        await sut.InvokeOnInitializedAsync();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetUnitsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(s => s.Add(error.Message, Severity.Error, null, null), Times.Once);
    }

    private sealed class TestableStatement : Statement
    {
        public Task InvokeOnInitializedAsync() => OnInitializedAsync();
    }
}
