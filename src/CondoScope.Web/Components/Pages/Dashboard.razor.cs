using MediatR;
using Microsoft.AspNetCore.Components;
using CondoScope.Application.Ledger.Queries.GetLedger;

namespace CondoScope.Web.Components.Pages;

public partial class Dashboard
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    
    private IReadOnlyList<LedgerDTO> ledger = [];
    protected override async Task OnInitializedAsync()
    {
        var ledgerResult = await Mediator.Send(new GetLedgerQuery());
        ledger = ledgerResult.Value ?? [];

        await base.OnInitializedAsync();
    }
}
