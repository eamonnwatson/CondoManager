using CondoScope.Application.Common.Interfaces;
using FluentResults;
using FluentResults.Extensions;
using MediatR;
using Unit = CondoScope.Domain.Entities.Unit;

namespace CondoScope.Application.Units.Commands;

internal class CreateUnitCommandHandler(IUnitsRepository unitsRepository) : IRequestHandler<CreateUnitCommand, Result<UnitDto>>
{
    public async Task<Result<UnitDto>> Handle(CreateUnitCommand request, CancellationToken cancellationToken) =>
        await Unit.Create(request.UnitNumber, request.Address, true, "System")
            .Bind(unit => unitsRepository.AddAsync(unit, cancellationToken))
            .Map(unit => new UnitDto(unit.Id.ToString(), unit.UnitNumber, unit.Address, string.Empty, string.Empty));
}
