using FluentResults;
using MediatR;

namespace CondoScope.Application.Owners.Queries.GetOwners;

public record GetOwnersQuery : IRequest<Result<IEnumerable<OwnerDto>>>;
