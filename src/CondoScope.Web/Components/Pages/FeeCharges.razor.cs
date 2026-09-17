using CondoScope.Application.FeeCharges;
using CondoScope.Application.FeeCharges.Commands.CreateFeeCharge;
using CondoScope.Application.FeeCharges.Queries.GetFeeCharges;
using CondoScope.Application.Owners.Commands.CreateOwner;
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

public partial class FeeCharges
{
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] internal IMediator Mediator { get; set; } = default!;
    [Inject] internal ISnackbar Snackbar { get; set; } = default!;

    private List<FeeChargeDto> feeCharges = default!;

    override protected async Task OnInitializedAsync()
    {
        var feeChargeResult = await Mediator.Send(new GetFeeChargesQuery());
        if (feeChargeResult.ShowErrorsIfFailed(Snackbar))
            return;

        feeCharges = feeChargeResult.Value.ToList();
    }

    private static string GetAppliesToString(FeeChargeDto feeCharge)
    {
        if (feeCharge.Scope == ChargeScope.AllUnits)
            return "All Units";

        return string.Join(", ", feeCharge.Units.Select(u => u.UnitNumber).Order());
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

        var parameters = new DialogParameters<AddFeeChargeDialog>()
        {
            { nameof(AddFeeChargeDialog.Units), units.Value.ToList() }
        };

        var dialog = await DialogService.ShowAsync<AddFeeChargeDialog>(parameters, options);
        var result = await dialog.Result;

        if (result is null || result.Canceled)
            return;

        if (result.Data is not AddFeeChargeFormModel newFeeCharge)
            return;

        var addResult = await Mediator.Send(
            new CreateFeeChargeCommand(
                DueDate: DateOnly.FromDateTime(newFeeCharge.DueDate ?? DateTime.Now),
                Description: newFeeCharge.Description,
                Amount: newFeeCharge.Amount,
                Category: (ChargeCategory)newFeeCharge.Category,
                Scope: (ChargeScope)newFeeCharge.Scope,
                UnitId: newFeeCharge.UnitId ?? string.Empty));

        if (addResult.ShowErrorsIfFailed(Snackbar))
            return;

        feeCharges.Add(addResult.Value);

    }


}
