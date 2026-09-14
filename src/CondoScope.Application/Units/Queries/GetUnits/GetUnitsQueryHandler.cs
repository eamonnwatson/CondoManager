using CondoScope.Application.Common;
using CondoScope.Application.Common.Interfaces;
using FluentResults;
using MediatR;

namespace CondoScope.Application.Units.Queries.GetUnits;

internal class GetUnitsQueryHandler(IUnitsRepository unitsRepository) : IRequestHandler<GetUnitsQuery, Result<IReadOnlyList<UnitDto>>>
{
    private readonly IUnitsRepository unitsRepository = unitsRepository;

    public async Task<Result<IReadOnlyList<UnitDto>>> Handle(GetUnitsQuery request, CancellationToken cancellationToken)
    {
        return await unitsRepository.GetAllWithCurrentOwner(cancellationToken)
            .MapAsync(units => (IReadOnlyList<UnitDto>)units.Select(unit => new UnitDto(
                UnitId: unit.Id.ToString(),
                UnitNumber: unit.UnitNumber,
                Address: unit.Address ?? string.Empty,
                CurrentOwnerId: unit.CurrentOwner?.Id.ToString() ?? string.Empty,
                CurrentOwnerName: unit.CurrentOwner?.Name ?? string.Empty
                )).ToList().AsReadOnly());
    }
}
