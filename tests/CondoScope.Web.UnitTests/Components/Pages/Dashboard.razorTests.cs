using System.Runtime.ExceptionServices;
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
public class DashboardRazorTests
{
    /// <summary>
    /// Minimal renderer that just enables driving the real Blazor component lifecycle
    /// (which is how the framework itself performs [Inject] property population),
    /// without pulling in a full-blown testing library such as bUnit.
    /// </summary>
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
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
    }

    /// <summary>
    /// Exposes the protected ChartValues/ChartLabels members through public wrappers so tests
    /// can observe them without reflection.
    /// </summary>
    private sealed class TestableDashboard : Dashboard
    {
        public decimal[] ExposedChartValues => ChartValues;

        public string[] ExposedChartLabels => ChartLabels;
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
    public async Task OnInitializedAsync_WhenQuerySucceeds_PopulatesLedgerAndChartData()
    {
        // Arrange
        var ledgerEntries = new List<LedgerDTO>
        {
            new("101", "Owner One", AccountStatus.PaidInFull, 100m, 50m, 0m, 100m, 0m),
            new("102", "Owner Two", AccountStatus.Outstanding, 200m, 30m, 0m, 200m, 40m),
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<LedgerDTO>>(ledgerEntries));

        using var renderer = CreateRenderer();

        // Act
        var dashboard = await renderer.InitializeComponentAsync<TestableDashboard>();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(
            s => s.Add(It.IsAny<string>(), It.IsAny<Severity>(), It.IsAny<Action<SnackbarOptions>?>(), It.IsAny<string?>()),
            Times.Never);

        var expectedCollected = ledgerEntries.Sum(l => l.Payments);
        var expectedOutstanding = ledgerEntries.Where(l => l.Balance > 0).Sum(l => l.Balance);

        CollectionAssert.AreEqual(new[] { expectedCollected, expectedOutstanding }, dashboard.ExposedChartValues);
        CollectionAssert.AreEqual(new[] { "Collected", "Outstanding" }, dashboard.ExposedChartLabels);
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
        var dashboard = await renderer.InitializeComponentAsync<TestableDashboard>();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(s => s.Add(errorMessage, Severity.Error, null, null), Times.Once);

        // Ledger remains empty (default), so both derived chart values are zero.
        CollectionAssert.AreEqual(new[] { 0m, 0m }, dashboard.ExposedChartValues);
        CollectionAssert.AreEqual(new[] { "Collected", "Outstanding" }, dashboard.ExposedChartLabels);
    }

    [TestMethod]
    public async Task OnInitializedAsync_WhenLedgerHasNoOutstandingBalances_OutstandingChartValueIsZero()
    {
        // Arrange
        var ledgerEntries = new List<LedgerDTO>
        {
            new("201", "Owner Three", AccountStatus.PaidInFull, 150m, 0m, 0m, 150m, 0m),
            new("202", "Owner Four", AccountStatus.Credit, 75m, 0m, 0m, 75m, -10m),
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<LedgerDTO>>(ledgerEntries));

        using var renderer = CreateRenderer();

        // Act
        var dashboard = await renderer.InitializeComponentAsync<TestableDashboard>();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()), Times.Once);

        var expectedCollected = ledgerEntries.Sum(l => l.Payments);
        CollectionAssert.AreEqual(new[] { expectedCollected, 0m }, dashboard.ExposedChartValues);
    }
}
#pragma warning restore BL0006
