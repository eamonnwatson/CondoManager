using FluentResults;
using MediatR;

namespace CondoScope.Application.Units.Queries.GetUnits;

public record GetUnitsQuery : IRequest<Result<IEnumerable<UnitDto>>>;
