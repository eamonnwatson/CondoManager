using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Ledger;
using CondoScope.Application.Ledger.Queries.GetLedger;
using CondoScope.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CondoScope.Web.Components.Pages;

public partial class Dashboard
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IMapper Mapper { get; set; } = default!;


    protected decimal[] ChartValues => [ledger.Sum(l => l.Payments), ledger.Where(l => l.Balance > 0).Sum(l => l.Balance)];
    protected string[] ChartLabels => ["Collected", "Outstanding"];

    private List<LedgerDTO> ledger = [];
    protected int ledgerCount;
    protected decimal totalCollected;
    protected decimal totalFees;
    protected decimal collectionPercentage;

    private readonly PieChartOptions chartOptions = new() { ChartPalette = ["#4caf50", "#f44336"] };

    protected override async Task OnInitializedAsync()
    {
        var ledgerResult = await Mediator.Send(new GetLedgerQuery());
        if (ledgerResult.ShowErrorsIfFailed(Snackbar))
            return;

        ledger = ledgerResult.Value.ToList() ?? [];
        ledgerCount = ledger.Count;
        totalCollected = ledger.Sum(l => l.Payments);
        totalFees = ledger.Where(l => l.Balance > 0).Sum(l => l.Balance);
        collectionPercentage = totalCollected > 0 ? (totalCollected - totalFees) / totalCollected : 0;

    }

    private string GetAccountStatus(AccountStatus status) =>
        Mapper.Map<string>(status);

    private static Color GetColor(AccountStatus status)
    {
        return status switch
        {
            AccountStatus.PaidInFull => Color.Success,
            AccountStatus.Credit => Color.Info,
            AccountStatus.Outstanding => Color.Error,
            _ => Color.Default
        };
    }
}
