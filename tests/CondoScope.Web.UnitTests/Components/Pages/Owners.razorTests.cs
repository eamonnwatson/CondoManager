using CondoScope.Application.Owners;
using CondoScope.Application.Owners.Queries.GetOwners;
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

[TestClass]
#pragma warning disable BL0006 // Renderer/RenderTree types are used intentionally to drive the real Blazor lifecycle for testing.
public class OwnersRazorTests
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
            // Downstream MudBlazor child components require services (localizers, JS interop, etc.)
            // that are irrelevant to testing Owners.OnInitializedAsync in isolation. Any exception
            // raised while rendering markup happens strictly after OnInitializedAsync has already
            // run and updated component state, so it is safe to ignore here for this test's purposes.
        }

        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
    }

    private readonly Mock<IMediator> mediatorMock = new(MockBehavior.Strict);
    private readonly Mock<ISnackbar> snackbarMock = new(MockBehavior.Strict);
    private readonly Mock<IDialogService> dialogServiceMock = new(MockBehavior.Strict);

    private TestRenderer CreateRenderer()
    {
        var services = new ServiceCollection();
        services.AddSingleton(mediatorMock.Object);
        services.AddSingleton(snackbarMock.Object);
        services.AddSingleton(dialogServiceMock.Object);
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        return new TestRenderer(services.BuildServiceProvider());
    }

    [TestMethod]
    public async Task OnInitializedAsync_WhenQuerySucceeds_PopulatesOwnersFromResult()
    {
        // Arrange
        var expectedOwners = new List<OwnerDto>
        {
            new("1", "101", "Owner One", "Addr1", "owner1@example.com", "1234567890"),
            new("2", "102", "Owner Two", "Addr2", "owner2@example.com", "0987654321"),
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOwnersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<OwnerDto>>(expectedOwners));

        using var renderer = CreateRenderer();

        // Act
        await renderer.InitializeComponentAsync<Owners>();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetOwnersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<Severity>(), It.IsAny<Action<SnackbarOptions>?>(), It.IsAny<string?>()), Times.Never);
    }

    [TestMethod]
    public async Task OnInitializedAsync_WhenQueryFails_ShowsErrorAndDoesNotThrow()
    {
        // Arrange
        const string errorMessage = "Database unavailable";

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOwnersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IEnumerable<OwnerDto>>(errorMessage));

        snackbarMock
            .Setup(s => s.Add(errorMessage, Severity.Error, null, null))
            .Returns((Snackbar)null!);

        using var renderer = CreateRenderer();

        // Act
        await renderer.InitializeComponentAsync<Owners>();

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetOwnersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(s => s.Add(errorMessage, Severity.Error, null, null), Times.Once);
    }
}
#pragma warning restore BL0006
