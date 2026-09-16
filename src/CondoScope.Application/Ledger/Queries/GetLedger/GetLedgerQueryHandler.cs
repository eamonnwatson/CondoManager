using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using FluentResults;
using FluentResults.Extensions;
using MediatR;

namespace CondoScope.Application.Ledger.Queries.GetLedger;

internal class GetLedgerQueryHandler(IUnitsRepository unitsRepository, IMapper mapper) : IRequestHandler<GetLedgerQuery, Result<IEnumerable<LedgerDTO>>>
{
    public async Task<Result<IEnumerable<LedgerDTO>>> Handle(GetLedgerQuery request, CancellationToken cancellationToken) =>
        await unitsRepository.GetAllWithDetailsAsync(cancellationToken)
            .Map(mapper.Map<LedgerDTO>);
}
