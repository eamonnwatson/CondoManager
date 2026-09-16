using CondoScope.Application.Ledger;
using CondoScope.Application.Ledger.Queries.GetLedger;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CondoScope.Web.Components.Pages;

public partial class Dashboard
{
    [Inject] private IMediator Mediator { get; set; } = default!;

    protected decimal[] ChartValues => [ledger.Sum(l => l.Payments), ledger.Where(l => l.Balance > 0).Sum(l => l.Balance)];
    protected string[] ChartLabels => ["Collected", "Outstanding"];

    private List<LedgerDTO> ledger = [];
    private int ledgerCount;
    private decimal totalCollected;
    private decimal totalFees;
    private decimal collectionPercentage;

    private readonly PieChartOptions chartOptions = new() { ChartPalette = ["#4caf50", "#f44336"] };

    protected override async Task OnInitializedAsync()
    {
        var ledgerResult = await Mediator.Send(new GetLedgerQuery());
        ledger = ledgerResult.Value.ToList() ?? [];
        ledgerCount = ledger.Count;
        totalCollected = ledger.Sum(l => l.Payments);
        totalFees = ledger.Where(l => l.Balance > 0).Sum(l => l.Balance);
        collectionPercentage = totalCollected > 0 ? (totalCollected - totalFees) / totalCollected : 0;

        await base.OnInitializedAsync();
    }
}
