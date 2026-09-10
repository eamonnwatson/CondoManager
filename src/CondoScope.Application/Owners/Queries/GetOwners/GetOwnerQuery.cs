using FluentResults;
using MediatR;

namespace CondoScope.Application.Owners.Queries.GetOwners;

public record GetOwnerQuery() : IRequest<Result<IReadOnlyList<OwnerDTO>>>;
