using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Units;
using CondoScope.Domain.Entities;

namespace CondoScope.Application.FeeCharges;

internal class FeeChargeMapper : IMapModule
{
    public void RegisterMaps(IMapper mapper)
    {
        mapper.Register<FeeCharge, FeeChargeDto>(fc => new FeeChargeDto(
            Id: fc.Id.ToString(),
            DueDate: fc.DueDate,
            Description: fc.Description,
            Category: fc.Category,
            Scope: fc.Scope,
            Units: mapper.Map<UnitDto>(fc.Units),
            Amount: fc.Amount));
    }
}
