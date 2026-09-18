using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Ledger;
using CondoScope.Application.Ledger.Queries.GetLedger;
using CondoScope.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CondoScope.Web.Components.Pages;

public partial class Receivable
{
    private List<LedgerDTO> ledger = [];

    [Inject] public IMediator Mediator { get; set; } = default!; 
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IMapper Mapper { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var ledgerResult = await Mediator.Send(new GetLedgerQuery());
        if (ledgerResult.ShowErrorsIfFailed(Snackbar))
            return;

        ledger = ledgerResult.Value.OrderBy(l => l.UnitNumber).ToList() ?? [];

    }

    private string GetAccountStatus(AccountStatus status) =>
        Mapper.Map<string>(status);

}
