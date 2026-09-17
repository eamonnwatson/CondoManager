using CondoScope.Application.Owners.Commands.CreateOwner;
using CondoScope.Application.Payments;
using CondoScope.Application.Payments.Commands.CreatePayment;
using CondoScope.Application.Payments.Queries.GetPayments;
using CondoScope.Application.Units.Queries.GetUnits;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using CondoScope.Web.Components.Dialogs;
using CondoScope.Web.Components.Dialogs.Models;
using CondoScope.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CondoScope.Web.Components.Pages;

public partial class Payments
{
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;


    private List<PaymentDto> payments = default!;

    override protected async Task OnInitializedAsync()
    {
        var paymentResult = await Mediator.Send(new GetPaymentsQuery());
        if (paymentResult.ShowErrorsIfFailed(Snackbar))
            return;

        payments = paymentResult.Value.ToList();
    }

    private async Task OpenCreateDialog()
    {
        var options = new DialogOptions()
        {
            FullWidth = true,
            MaxWidth = MaxWidth.Small
        };

        var units = await Mediator.Send(new GetUnitsQuery());
        if (units.ShowErrorsIfFailed(Snackbar))
            return;

        var parameters = new DialogParameters<AddPaymentDialog>()
        {
            { nameof(AddPaymentDialog.Units), units.Value.ToList() }
        };

        var dialog = await DialogService.ShowAsync<AddPaymentDialog>(parameters, options);
        var result = await dialog.Result;

        if (result is null || result.Canceled)
            return;

        if (result.Data is not AddPaymentFormModel newPayment)
            return;

        var addResult = await Mediator.Send(
            new CreatePaymentCommand(
                PaymentDate: DateOnly.FromDateTime(newPayment.PaymentDate ?? DateTime.Today),
                UnitId: newPayment.UnitId ?? string.Empty,
                Amount: newPayment.Amount ?? 0,
                PaymentMethod: (PaymentMethod)(newPayment.PaymentMethod ?? 0),
                ReferenceNumber: newPayment.Reference,
                Notes: newPayment.Notes));

        if (addResult.ShowErrorsIfFailed(Snackbar))
            return;

        payments.Add(addResult.Value);

    }


}
