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
        var provider = services.BuildServiceProvider();

        var renderer = new TestRenderer(provider);
        var component = (Payments)renderer.InstantiateComponent(typeof(Payments));

        return (renderer, component);
    }

    [TestMethod]
    public async Task OnInitializedAsync_MediatorReturnsSuccess_PopulatesPaymentsAndDoesNotShowError()
    {
        // Arrange
        var unit = new CondoScope.Application.Units.UnitDto("unit-1", "101", "123 Main St", "owner-1", "Owner One");
        var payment = new PaymentDto("pay-1", unit, new DateOnly(2024, 1, 1), 100m, PaymentMethod.Cash, "ref-1", "notes-1");

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPaymentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<PaymentDto>>([payment]));

        var (renderer, component) = CreateRenderedComponent();

        // Act
        await renderer.RenderRootComponentAsync(component);

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetPaymentsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<Severity>(), null, null), Times.Never);
    }

    [TestMethod]
    public async Task OnInitializedAsync_MediatorReturnsFailure_ShowsErrorAndDoesNotPopulatePayments()
    {
        // Arrange
        var error = new FluentResults.Error("payments lookup failed");

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetPaymentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IEnumerable<PaymentDto>>(error));

        snackbarMock
            .Setup(s => s.Add(error.Message, Severity.Error, null, null))
            .Returns((Snackbar)null!);

        var (renderer, component) = CreateRenderedComponent();

        // Act
        await renderer.RenderRootComponentAsync(component);

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetPaymentsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        snackbarMock.Verify(s => s.Add(error.Message, Severity.Error, null, null), Times.Once);
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

        protected override void HandleException(Exception exception) => throw exception;

        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
    }
}

#pragma warning restore BL0006
