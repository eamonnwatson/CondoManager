using CondoScope.Application.Common.Mapping;
using CondoScope.Domain.Entities;

namespace CondoScope.Application.Units;

internal class UnitMapper : IMapModule
{
    public void RegisterMaps(IMapper mapper)
    {
        mapper.Register<Unit, UnitDto>(unit => new UnitDto(
            UnitId: unit.Id.ToString(),
            UnitNumber: unit.UnitNumber,
            Address: unit.Address ?? string.Empty,
            CurrentOwnerId: unit.CurrentOwner?.Id.ToString() ?? string.Empty,
            CurrentOwnerName: unit.CurrentOwner?.Name ?? string.Empty));
    }
}
