using CondoScope.Application.Units.Queries;
using FluentResults;
using MediatR;

namespace CondoScope.Application.Units.Commands;

public record CreateUnitCommand(string UnitNumber, string Address) : IRequest<Result<UnitDto>>;
