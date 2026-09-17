using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using FluentResults;
using FluentResults.Extensions;
using MediatR;
namespace CondoScope.Application.FeeCharges.Commands.CreateFeeCharge;

internal class CreateFeeChargeCommandHandler(IFeeChargeRepository feeChargeRepository, IUnitsRepository unitsRepository, IMapper mapper) : IRequestHandler<CreateFeeChargeCommand, Result<FeeChargeDto>>
{
    public async Task<Result<FeeChargeDto>> Handle(CreateFeeChargeCommand request, CancellationToken cancellationToken) =>
        await unitsRepository.GetAllWithCurrentOwnerAsync(cancellationToken)
            .Bind(units => FeeCharge.Create(
                amount: request.Amount, 
                dueDate: request.DueDate, 
                description: request.Description, 
                category: request.Category, 
                scope: request.Scope, 
                unit: request.Scope == ChargeScope.SpecificUnit 
                        ? units.SingleOrDefault(u => u.Id.ToString() == request.UnitId) 
                        : null,
                createdBy: "System", 
                allUnits: units))
            .Bind(fc => feeChargeRepository.AddAsync(fc, cancellationToken))
            .Map(mapper.Map<FeeChargeDto>);
}
