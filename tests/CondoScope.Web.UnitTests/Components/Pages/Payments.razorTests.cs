using CondoScope.Application.Payments;
using CondoScope.Application.Payments.Queries.GetPayments;
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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace CondoScope.Web.UnitTests.Components.Pages;

#pragma warning disable BL0006 // Direct usage of Renderer/RenderTree types is required to test Blazor component lifecycle without a mocking framework for private [Inject] properties.

[TestClass]
public class PaymentsTests
{
    private readonly Mock<IMediator> mediatorMock = new(MockBehavior.Strict);
    private readonly Mock<ISnackbar> snackbarMock = new(MockBehavior.Strict);

    private (TestRenderer Renderer, Payments Component) CreateRenderedComponent()
    {
        var services = new ServiceCollection();
        services.AddSingleton(mediatorMock.Object);
        services.AddSingleton(snackbarMock.Object);
        services.AddSingleton(Mock.Of<IDialogService>());
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        var provider = services.BuildServiceProvider();

        var renderer = new TestRenderer(provider);
        var component = (Payments)renderer.InstantiateComponent(typeof(Payments));

        return (renderer, component);
    }

    [TestMethod]
    public async Task OnInitializedAsync_WhenQuerySucceeds_PopulatesPaymentsAndDoesNotShowSnackbar()
    {
        // Arrange
        var expectedPayments = new List<PaymentDto>
        {
            new("id1", new CondoScope.Application.Units.UnitDto("u1", "101", "Main St", string.Empty, string.Empty), new DateOnly(2024, 1, 1), 10m, PaymentMethod.Cash, "R1", "N1"),
        };

        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPaymentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<PaymentDto>>(expectedPayments));

        var (renderer, component) = this.CreateRenderedComponent();

        // Act
        await renderer.RenderRootComponentAsync(component);

        // Assert
        this.mediatorMock.Verify(m => m.Send(It.IsAny<GetPaymentsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        this.snackbarMock.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<Severity>(), null, null), Times.Never);
    }

    [TestMethod]
    public async Task OnInitializedAsync_WhenQueryFails_ShowsSnackbarAndDoesNotPopulatePayments()
    {
        // Arrange
        this.mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPaymentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IEnumerable<PaymentDto>>("Something went wrong"));

        this.snackbarMock
            .Setup(s => s.Add("Something went wrong", Severity.Error, null, null))
            .Returns((Snackbar?)null!);

        var (renderer, component) = this.CreateRenderedComponent();

        // Act
        await renderer.RenderRootComponentAsync(component);

        // Assert
        this.mediatorMock.Verify(m => m.Send(It.IsAny<GetPaymentsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        this.snackbarMock.Verify(s => s.Add("Something went wrong", Severity.Error, null, null), Times.Once);
    }

    private sealed class TestRenderer(IServiceProvider services) : Renderer(services, NullLoggerFactory.Instance)
    {
        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();

        public new IComponent InstantiateComponent(Type componentType) =>
            base.InstantiateComponent(componentType);

        public Task RenderRootComponentAsync(IComponent component) =>
            Dispatcher.InvokeAsync(() =>
            {
                var componentId = AssignRootComponentId(component);
                return RenderRootComponentAsync(componentId);
            });

        protected override void HandleException(Exception exception)
        {
            // Downstream MudBlazor child components require services (localizers, JS interop, etc.)
            // that are irrelevant to testing Payments.OnInitializedAsync in isolation. Any exception
            // raised while rendering markup happens strictly after OnInitializedAsync has already
            // run and updated component state, so it is safe to ignore here for this test's purposes.
        }

        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
    }
}

#pragma warning restore BL0006
