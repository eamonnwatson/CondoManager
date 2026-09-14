using CondoScope.Application.Common;
using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Units.Queries;
using FluentResults;
using MediatR;

namespace CondoScope.Application.Units.Commands;

internal class CreateUnitCommandHandler(IUnitsRepository unitsRepository) : IRequestHandler<CreateUnitCommand, Result<UnitDto>>
{
    private readonly IUnitsRepository unitsRepository = unitsRepository;

    public async Task<Result<UnitDto>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        return await Domain.Entities.Unit.Create(request.UnitNumber, request.Address, true, "System")
            .BindAsync(unit => unitsRepository.AddUnitAsync(unit, cancellationToken))
            .MapAsync(unit => new UnitDto(unit.Id.ToString(), unit.UnitNumber, unit.Address, string.Empty, string.Empty));
    }
}
