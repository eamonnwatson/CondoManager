using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Ledger;
using CondoScope.Application.Statement;
using CondoScope.Application.Statement.Queries;
using CondoScope.Application.Units;
using CondoScope.Application.Units.Queries.GetUnits;
using CondoScope.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace CondoScope.Web.Components.Pages;

public partial class Statement
{
    [Inject] public IMediator Mediator { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IMapper Mapper { get; set; } = default!;


    private List<UnitDto> units = [];
    private string selectedUnit = string.Empty;

    private StatementDto? statement;

    protected override async Task OnInitializedAsync()
    {
        var unitResult = await Mediator.Send(new GetUnitsQuery());
        if (unitResult.ShowErrorsIfFailed(Snackbar))
            return;

        units = unitResult.Value.ToList();

    }

    private async Task OnUnitChanged()
    {
        var statementResult = await Mediator.Send(new GetStatementQuery(selectedUnit));
        if (statementResult.ShowErrorsIfFailed(Snackbar))
            return;

        statement = statementResult.Value;

    }

    private async Task PrintStatement()
    {
        if (statement is null)
            return;

        var pdfUrl = NavigationManager.ToAbsoluteUri($"/statement-pdf/{selectedUnit}").ToString();

        await JsRuntime.InvokeVoidAsync("open", pdfUrl, "_blank");
    }

    private string GetAccountStatus(AccountStatus? status)
    {
        if (status is null)
            return string.Empty;

        return Mapper.Map<string>(status);
    }
    
    private static Color GetColor(AccountStatus? status)
    {
        if (status is null)
            return Color.Default;

        return status switch
        {
            AccountStatus.Credit => Color.Info,
            AccountStatus.PaidInFull => Color.Success,
            AccountStatus.Outstanding => Color.Error,
            _ => Color.Default
        };

    }

}
