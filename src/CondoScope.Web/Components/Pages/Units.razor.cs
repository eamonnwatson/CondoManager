using CondoScope.Application.Units;
using CondoScope.Application.Units.Commands;
using CondoScope.Application.Units.Queries.GetUnits;
using CondoScope.Web.Components.Dialogs;
using CondoScope.Web.Components.Dialogs.Models;
using CondoScope.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CondoScope.Web.Components.Pages;

public partial class Units
{

    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;


    private List<UnitDto> units = default!;

    override protected async Task OnInitializedAsync()
    {
        var unitResult = await Mediator.Send(new GetUnitsQuery());
        if (unitResult.ShowErrorsIfFailed(Snackbar))
            return;

        units = unitResult.Value.ToList();
    }

    private async Task OpenCreateDialog()
    {
        var options = new DialogOptions()
        {
            FullWidth = true,
            MaxWidth = MaxWidth.Small
        };

        var dialog = await DialogService.ShowAsync<AddUnitDialog>(options);
        var result = await dialog.Result;

        if (result is null || result.Canceled)
            return;

        if (result.Data is not AddUnitFormModel newUnit)
            return;

        var addResult = await Mediator.Send(new CreateUnitCommand(newUnit.UnitNumber, newUnit.Address));
        if (addResult.ShowErrorsIfFailed(Snackbar))
            return;

        units.Add(addResult.Value);

        
    }
}
