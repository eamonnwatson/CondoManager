using CondoScope.Application.Common;
using CondoScope.Application.Common.Interfaces;
using FluentResults;
using MediatR;

namespace CondoScope.Application.FeeCharges.Queries.GetFeeCharges;

public class GetFeeChargesQueryHandler(IFeeChargeRepository feeChargeRepository) : IRequestHandler<GetFeeChargesQuery, Result<IReadOnlyList<FeeChargeDto>>>
{
    private readonly IFeeChargeRepository feeChargeRepository = feeChargeRepository;

    public async Task<Result<IReadOnlyList<FeeChargeDto>>> Handle(GetFeeChargesQuery request, CancellationToken cancellationToken)
    {

        return await feeChargeRepository.GetAllAsync(cancellationToken)
            .MapAsync(a => (IReadOnlyList<FeeChargeDto>)a.Select(fc => new FeeChargeDto(
                Id: fc.Id.ToString(), 
                DueDate: fc.DueDate, 
                Description: fc.Description, 
                Category: fc.Category.ToString(), 
                AppliesTo: fc.Scope == Domain.Enums.ChargeScope.AllUnits ? "All Units" : string.Join(", ", fc.Units.Select(u => u.UnitNumber)), 
                Amount: fc.Amount)).ToList().AsReadOnly());

    }
}
