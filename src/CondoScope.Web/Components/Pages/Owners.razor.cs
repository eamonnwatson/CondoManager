using CondoScope.Application.Owners;
using CondoScope.Application.Owners.Commands.CreateOwner;
using CondoScope.Application.Owners.Queries.GetOwners;
using CondoScope.Application.Units.Queries.GetUnits;
using CondoScope.Web.Components.Dialogs;
using CondoScope.Web.Components.Dialogs.Models;
using CondoScope.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CondoScope.Web.Components.Pages;

public partial class Owners
{
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;


    private IList<OwnerDto> owners = default!;

    override protected async Task OnInitializedAsync()
    {
        var ownerResult = await Mediator.Send(new GetOwnersQuery());
        if (ownerResult.ShowErrorsIfFailed(Snackbar))
            return;

        owners = ownerResult.Value.ToList();
    }

    private async Task OpenCreateDialog()
    {
        var options = new DialogOptions()
        {
            FullWidth = true,
            MaxWidth = MaxWidth.Small
        };

        var units = await Mediator.Send(new GetUnitsQuery(OnlyUnitsWithNoOwners: true));
        if (units.ShowErrorsIfFailed(Snackbar))
            return;

        var parameters = new DialogParameters<AddOwnerDialog>()
        {
            { nameof(AddOwnerDialog.Units), units.Value.ToList() }
        };

        var dialog = await DialogService.ShowAsync<AddOwnerDialog>(parameters, options);
        var result = await dialog.Result;

        if (result is null || result.Canceled)
            return;

        if (result.Data is not AddOwnerFormModel newOwner)
            return;

        var addResult = await Mediator.Send(
            new CreateOwnerCommand(
                newOwner.Name,
                newOwner.EmailAddress,
                newOwner.PhoneNumber,
                newOwner.UnitId,
                newOwner.EffectiveDate.HasValue
                    ? DateOnly.FromDateTime(newOwner.EffectiveDate.Value)
                    : null));
        if (addResult.ShowErrorsIfFailed(Snackbar))
            return;

        owners.Add(addResult.Value);

    }

    private static string FormatPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (digits.Length == 10)
            return $"({digits[..3]}) {digits[3..6]}-{digits[6..]}";

        return phoneNumber;
    }

}
