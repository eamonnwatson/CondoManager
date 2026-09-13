using FluentResults;
using MediatR;

namespace CondoScope.Application.Units.Queries;

public record GetUnitsQuery() : IRequest<Result<IReadOnlyList<UnitDto>>>;
