using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using FluentResults;
using FluentResults.Extensions;
using MediatR;

namespace CondoScope.Application.Units.Queries.GetUnits;

internal class GetUnitsQueryHandler(IUnitsRepository unitsRepository, IMapper mapper) : IRequestHandler<GetUnitsQuery, Result<IEnumerable<UnitDto>>>
{
    public async Task<Result<IEnumerable<UnitDto>>> Handle(GetUnitsQuery request, CancellationToken cancellationToken) =>
        await unitsRepository.GetAllWithCurrentOwner(cancellationToken)
            .Map(mapper.Map<UnitDto>);
}
