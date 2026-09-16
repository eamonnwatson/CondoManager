using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using FluentResults;
using FluentResults.Extensions;
using MediatR;

namespace CondoScope.Application.Owners.Queries.GetOwners;

internal class GetOwnersQueryHandler(IOwnersRepository ownersRepository, IMapper mapper) : IRequestHandler<GetOwnersQuery, Result<IEnumerable<OwnerDto>>>
{
    public async Task<Result<IEnumerable<OwnerDto>>> Handle(GetOwnersQuery request, CancellationToken cancellationToken) =>
        await ownersRepository.GetAllAsync(cancellationToken)
            .Map(mapper.Map<OwnerDto>);
}
