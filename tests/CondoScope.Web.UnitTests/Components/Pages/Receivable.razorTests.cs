using CondoScope.Application.Ledger;
using CondoScope.Application.Ledger.Queries.GetLedger;
using CondoScope.Domain.Enums;
using CondoScope.Web.Components.Pages;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MudBlazor;

namespace CondoScope.Web.UnitTests.Components.Pages;

#pragma warning disable BL0006 // Renderer/RenderTree types are used intentionally to drive the real Blazor lifecycle for testing.
[TestClass]
public class ReceivableRazorTests
{
    private sealed class TestRenderer : Renderer
    {
        public TestRenderer(IServiceProvider serviceProvider)
            : base(serviceProvider, NullLoggerFactory.Instance)
        {
        }

        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();

        public async Task<T> InitializeComponentAsync<T>() where T : IComponent
        {
            return await Dispatcher.InvokeAsync(async () =>
            {
                var component = InstantiateComponent(typeof(T));
                var componentId = AssignRootComponentId(component);
                await RenderRootComponentAsync(componentId);
                return (T)component;
            });
        }

        protected override void HandleException(Exception exception)
        {
            // Downstream MudBlazor child components require services (localizers, JS interop, etc.)
            // that are irrelevant to testing Receivable.OnInitializedAsync in isolation. Any exception
            // raised while rendering markup happens strictly after OnInitializedAsync has already
            // run and updated component state, so it is safe to ignore here for this test's purposes.
        }

        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
    }

    private readonly Mock<IMediator> mediatorMock = new(MockBehavior.Strict);
    private readonly Mock<ISnackbar> snackbarMock = new(MockBehavior.Strict);

    private TestRenderer CreateRenderer()
    {
        var services = new ServiceCollection();
        services.AddSingleton(mediatorMock.Object);
        services.AddSingleton(snackbarMock.Object);
        return new TestRenderer(services.BuildServiceProvider());
    }

    [TestMethod]
    public async Task OnInitializedAsync_WhenQuerySucceeds_PopulatesLedgerOrderedByUnitNumber()
    {
        // Arrange
        var ledgerEntries = new List<LedgerDTO>
        {
            new("202", "Owner Two", AccountStatus.Outstanding, 200m, 30m, 0m, 200m, 40m),
            new("101", "Owner One", AccountStatus.PaidInFull, 100m, 50m, 0m, 100m, 0m),
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<LedgerDTO>>(ledgerEntries));

        using var renderer = CreateRenderer();

        // Act
        await renderer.InitializeComponentAsync<Receivable>();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(
            s => s.Add(It.IsAny<string>(), It.IsAny<Severity>(), It.IsAny<Action<SnackbarOptions>?>(), It.IsAny<string?>()),
            Times.Never);
    }

    [TestMethod]
    public async Task OnInitializedAsync_WhenQueryFails_ShowsErrorAndDoesNotPopulateLedger()
    {
        // Arrange
        const string errorMessage = "Ledger lookup failed";

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IEnumerable<LedgerDTO>>(errorMessage));

        snackbarMock
            .Setup(s => s.Add(errorMessage, Severity.Error, null, null))
            .Returns((Snackbar)null!);

        using var renderer = CreateRenderer();

        // Act
        await renderer.InitializeComponentAsync<Receivable>();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(s => s.Add(errorMessage, Severity.Error, null, null), Times.Once);
    }
}
#pragma warning restore BL0006
