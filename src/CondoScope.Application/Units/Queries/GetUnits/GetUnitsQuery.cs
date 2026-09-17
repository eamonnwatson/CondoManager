using FluentResults;
using MediatR;

namespace CondoScope.Application.Units.Queries.GetUnits;

public record GetUnitsQuery(bool OnlyUnitsWithNoOwners = false) : IRequest<Result<IEnumerable<UnitDto>>>;
