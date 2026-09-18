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
using MudBlazor.Services;

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

    /// <summary>
    /// Minimal NavigationManager implementation to satisfy component tree dependency injection
    /// requirements when child MudBlazor components (e.g., MudChip) require navigation services.
    /// </summary>
    private sealed class FakeNavigationManager : NavigationManager
    {
        public FakeNavigationManager() => Initialize("https://localhost/", "https://localhost/");

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
        }
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
    public async Task OnInitializedAsync_WhenLedgerIsEmpty_PopulatesEmptyChartData()
    {
        // Arrange
        var ledgerEntries = new List<LedgerDTO>();

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<LedgerDTO>>(ledgerEntries));

        var services = new ServiceCollection();
        services.AddSingleton(mediatorMock.Object);
        services.AddSingleton(snackbarMock.Object);
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(jsRuntimeMock.Object);
        services.AddSingleton(new Mock<CondoScope.Application.Common.Mapping.IMapper>(MockBehavior.Loose).Object);
        using var renderer = new TestRenderer(services.BuildServiceProvider());

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
    public async Task OnInitializedAsync_WhenLedgerHasEntries_PopulatesTotalsAndChartValues()
    {
        // Arrange
        var ledgerEntries = new List<LedgerDTO>
        {
            new("101", "Alice", CondoScope.Application.Ledger.AccountStatus.PaidInFull, 100m, 10m, 0m, 110m, 0m),
            new("102", "Bob", CondoScope.Application.Ledger.AccountStatus.Outstanding, 100m, 10m, 0m, 50m, 60m),
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<LedgerDTO>>(ledgerEntries));

        var services = new ServiceCollection();
        services.AddSingleton(mediatorMock.Object);
        services.AddSingleton(snackbarMock.Object);
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(jsRuntimeMock.Object);
        services.AddMudServices();
        services.AddSingleton(new Mock<CondoScope.Application.Common.Mapping.IMapper>(MockBehavior.Loose).Object);
        services.AddSingleton(new Mock<MudBlazor.IKeyInterceptorService>(MockBehavior.Loose).Object);
        services.AddSingleton<Microsoft.AspNetCore.Components.NavigationManager>(new FakeNavigationManager());
        using var renderer = new TestRenderer(services.BuildServiceProvider());

        // Act
        var dashboard = await renderer.InitializeComponentAsync<TestableDashboard>();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(
            s => s.Add(It.IsAny<string>(), It.IsAny<Severity>(), It.IsAny<Action<SnackbarOptions>?>(), It.IsAny<string?>()),
            Times.Never);

        Assert.AreEqual(2, dashboard.LedgerCount);
        Assert.AreEqual(160m, dashboard.TotalCollected);
        Assert.AreEqual(60m, dashboard.TotalFees);
        Assert.AreEqual((160m - 60m) / 160m, dashboard.CollectionPercentage);
        CollectionAssert.AreEqual(new[] { 160m, 60m }, dashboard.ExposedChartValues);
    }

    [TestMethod]
    public async Task OnInitializedAsync_WhenMediatorResultFails_ShowsErrorAndDoesNotPopulateLedger()
    {
        // Arrange
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IEnumerable<LedgerDTO>>("Ledger fetch failed"));

        snackbarMock
            .Setup(s => s.Add("Ledger fetch failed", Severity.Error, null, null))
            .Returns((Snackbar?)null!);

        var services = new ServiceCollection();
        services.AddSingleton(mediatorMock.Object);
        services.AddSingleton(snackbarMock.Object);
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(jsRuntimeMock.Object);
        services.AddSingleton(new Mock<CondoScope.Application.Common.Mapping.IMapper>(MockBehavior.Loose).Object);
        using var renderer = new TestRenderer(services.BuildServiceProvider());

        // Act
        var dashboard = await renderer.InitializeComponentAsync<TestableDashboard>();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetLedgerQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(s => s.Add("Ledger fetch failed", Severity.Error, null, null), Times.Once);

        // Ledger-related fields remain at their default values since the method returned early
        Assert.AreEqual(0, dashboard.LedgerCount);
        Assert.AreEqual(0m, dashboard.TotalCollected);
        Assert.AreEqual(0m, dashboard.TotalFees);
        Assert.AreEqual(0m, dashboard.CollectionPercentage);
    }

}
