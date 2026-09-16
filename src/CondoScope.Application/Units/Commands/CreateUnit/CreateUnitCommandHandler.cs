using CondoScope.Application.Common.Interfaces;
using FluentResults;
using FluentResults.Extensions;
using MediatR;

namespace CondoScope.Application.Units.Commands;

internal class CreateUnitCommandHandler(IUnitsRepository unitsRepository) : IRequestHandler<CreateUnitCommand, Result<UnitDto>>
{
    public async Task<Result<UnitDto>> Handle(CreateUnitCommand request, CancellationToken cancellationToken) =>
        await Domain.Entities.Unit.Create(request.UnitNumber, request.Address, true, "System")
            .Bind(unit => unitsRepository.AddUnitAsync(unit, cancellationToken))
            .Map(unit => new UnitDto(unit.Id.ToString(), unit.UnitNumber, unit.Address, string.Empty, string.Empty));
}
