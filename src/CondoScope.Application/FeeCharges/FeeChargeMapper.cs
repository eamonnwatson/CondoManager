using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Ledger;
using CondoScope.Application.Units;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;

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


        mapper.Register<ChargeCategory, string>(category => category switch
        {
            ChargeCategory.CondoFee => "Condo Fee",
            ChargeCategory.ReserveFee => "Reserve Fund",
            ChargeCategory.OtherFee => "Other Fee",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        });

        mapper.Register<ChargeScope, string>(scope => scope switch
        {
            ChargeScope.AllUnits => "All Units",
            ChargeScope.SpecificUnit => "Specific Unit",
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
        });

    }
}
