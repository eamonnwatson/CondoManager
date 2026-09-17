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

        public int LedgerCount => ledgerCount;

        public decimal TotalCollected => totalCollected;

        public decimal TotalFees => totalFees;

        public decimal CollectionPercentage => collectionPercentage;
    }

    private readonly Mock<IMediator> mediatorMock = new(MockBehavior.Strict);
    private readonly Mock<ISnackbar> snackbarMock = new(MockBehavior.Strict);
    // JSRuntime uses Loose: MudBlazor chart components make JS interop calls that don't need explicit setup for this component test
    private readonly Mock<Microsoft.JSInterop.IJSRuntime> jsRuntimeMock = new(MockBehavior.Loose);

    private TestRenderer CreateRenderer()
    {
        var services = new ServiceCollection();
        services.AddSingleton(mediatorMock.Object);
        services.AddSingleton(snackbarMock.Object);
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(jsRuntimeMock.Object);
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
            .Returns((Snackbar?)null!);

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
        // Note: Test data includes a credit situation (negative Balance = -10m) where the owner has overpaid.
        // Only accounts with Balance > 0 contribute to the Outstanding chart value.
        var ledgerEntries = new List<LedgerDTO>
        {
            new("201", "Owner Three", AccountStatus.PaidInFull, 150m, 0m, 0m, 150m, 0m),
            new("202", "Owner Four", AccountStatus.Credit, 75m, 0m, 0m, 75m, -10m), // Credit account: negative balance
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

    [TestMethod]
    public async Task OnInitializedAsync_WhenLedgerIsEmpty_PopulatesEmptyChartData()
    {
        // Arrange
        var ledgerEntries = new List<LedgerDTO>();

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

        // Both collected and outstanding should be zero for empty ledger
        CollectionAssert.AreEqual(new[] { 0m, 0m }, dashboard.ExposedChartValues);
        CollectionAssert.AreEqual(new[] { "Collected", "Outstanding" }, dashboard.ExposedChartLabels);
    }

    [TestMethod]
    public async Task OnInitializedAsync_WhenQuerySucceeds_ComputesLedgerCountAndTotalsCorrectly()
    {
        // Arrange
        var ledgerEntries = new List<LedgerDTO>
        {
            new("301", "Owner Five", AccountStatus.Outstanding, 500m, 100m, 10m, 250m, 360m),
            new("302", "Owner Six", AccountStatus.PaidInFull, 300m, 50m, 5m, 300m, 0m),
            new("303", "Owner Seven", AccountStatus.Outstanding, 400m, 75m, 8m, 200m, 183m),
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<LedgerDTO>>(ledgerEntries));

        using var renderer = CreateRenderer();

        // Act
        var dashboard = await renderer.InitializeComponentAsync<TestableDashboard>();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()), Times.Once);

        var expectedCollected = ledgerEntries.Sum(l => l.Payments); // 750m
        var expectedOutstanding = ledgerEntries.Where(l => l.Balance > 0).Sum(l => l.Balance); // 543m

        CollectionAssert.AreEqual(new[] { expectedCollected, expectedOutstanding }, dashboard.ExposedChartValues);
        Assert.AreEqual(3, dashboard.LedgerCount, "Ledger count should match the number of entries");
        Assert.AreEqual(750m, dashboard.TotalCollected, "Total collected should be sum of all Payments");
        Assert.AreEqual(543m, dashboard.TotalFees, "Total fees (outstanding) should be sum of positive Balances");
    }

    [TestMethod]
    public async Task OnInitializedAsync_WhenCollectionPercentageCanBeCalculated_ComputesCorrectly()
    {
        // Arrange
        var ledgerEntries = new List<LedgerDTO>
        {
            new("401", "Owner Eight", AccountStatus.Outstanding, 1000m, 100m, 0m, 800m, 300m),
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<LedgerDTO>>(ledgerEntries));

        using var renderer = CreateRenderer();

        // Act
        var dashboard = await renderer.InitializeComponentAsync<TestableDashboard>();

        // Assert
        var expectedCollected = 800m;
        var expectedOutstanding = 300m;
        var expectedPercentage = (expectedCollected - expectedOutstanding) / expectedCollected; // (800 - 300) / 800 = 0.625 = 62.5%

        Assert.AreEqual(expectedCollected, dashboard.TotalCollected);
        Assert.AreEqual(expectedOutstanding, dashboard.TotalFees);
        Assert.AreEqual(expectedPercentage, dashboard.CollectionPercentage, 0.0001m, "Collection percentage should be (collected - outstanding) / collected");
    }

    [TestMethod]
    public async Task OnInitializedAsync_WhenCollectedIsZero_CollectionPercentageIsZero()
    {
        // Arrange
        var ledgerEntries = new List<LedgerDTO>
        {
            new("501", "Owner Nine", AccountStatus.Outstanding, 500m, 50m, 0m, 0m, 500m),
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<LedgerDTO>>(ledgerEntries));

        using var renderer = CreateRenderer();

        // Act
        var dashboard = await renderer.InitializeComponentAsync<TestableDashboard>();

        // Assert
        Assert.AreEqual(0m, dashboard.TotalCollected, "No payments collected");
        Assert.AreEqual(500m, dashboard.TotalFees, "Full balance outstanding");
        Assert.AreEqual(0m, dashboard.CollectionPercentage, "Percentage should be 0 when no payments collected");
    }

    [TestMethod]
    public async Task OnInitializedAsync_WithVeryLargeNumbers_HandlesFinancialCalculationsWithoutOverflow()
    {
        // Arrange
        // Test with very large decimal values to ensure no financial calculation overflow or precision loss
        var largeAmount = decimal.MaxValue / 10; // Use a large but safe value
        var ledgerEntries = new List<LedgerDTO>
        {
            new("601", "Owner Large1", AccountStatus.Outstanding, largeAmount, largeAmount / 2, 0m, largeAmount * 0.9m, largeAmount * 0.15m),
            new("602", "Owner Large2", AccountStatus.Outstanding, largeAmount / 2, largeAmount / 4, 0m, largeAmount * 0.8m, largeAmount * 0.1m),
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<LedgerDTO>>(ledgerEntries));

        using var renderer = CreateRenderer();

        // Act & Assert - should complete without throwing OverflowException
        var dashboard = await renderer.InitializeComponentAsync<TestableDashboard>();

        var expectedCollected = ledgerEntries.Sum(l => l.Payments);
        var expectedOutstanding = ledgerEntries.Where(l => l.Balance > 0).Sum(l => l.Balance);

        Assert.IsNotNull(dashboard);
        CollectionAssert.AreEqual(new[] { expectedCollected, expectedOutstanding }, dashboard.ExposedChartValues);
        Assert.IsTrue(dashboard.TotalCollected >= 0, "Total collected should be non-negative");
        Assert.IsTrue(dashboard.TotalFees >= 0, "Total fees should be non-negative");
        // Percentage should be between 0 and 1 (inclusive) when valid
        Assert.IsTrue(dashboard.CollectionPercentage >= 0 && dashboard.CollectionPercentage <= 1, "Collection percentage should be between 0 and 1");
    }

    [TestMethod]
    public async Task OnInitializedAsync_WithMixedLargeAndSmallNumbers_MaintainsPrecision()
    {
        // Arrange
        // Test mixing very large and very small numbers to verify decimal precision
        var ledgerEntries = new List<LedgerDTO>
        {
            new("701", "Owner Precise1", AccountStatus.Outstanding, 999999.99m, 50000m, 1.23m, 500000.75m, 400000.50m),
            new("702", "Owner Precise2", AccountStatus.PaidInFull, 0.01m, 0.005m, 0m, 0.01m, 0m),
            new("703", "Owner Precise3", AccountStatus.Outstanding, 1000000.00m, 100000m, 0.50m, 750000.25m, 150000.75m),
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<LedgerDTO>>(ledgerEntries));

        using var renderer = CreateRenderer();

        // Act
        var dashboard = await renderer.InitializeComponentAsync<TestableDashboard>();

        // Assert
        // Sum of Payments: 500000.75 + 0.01 + 750000.25 = 1250001.01
        // Sum of positive Balances: 400000.50 + 150000.75 = 550001.25
        var expectedCollected = 1250001.01m;
        var expectedOutstanding = 550001.25m;

        Assert.AreEqual(expectedCollected, dashboard.TotalCollected, 0.01m, "Total collected should maintain precision");
        Assert.AreEqual(expectedOutstanding, dashboard.TotalFees, 0.01m, "Total fees should maintain precision");
    }
}
