using CondoScope.Application.Common;
using CondoScope.Application.Common.Interfaces;
using FluentResults;
using MediatR;

namespace CondoScope.Application.Owners.Queries.GetOwners;

internal class GetOwnerQueryHandler(IOwnersRepository ownersRepository) : IRequestHandler<GetOwnerQuery, Result<IReadOnlyList<OwnerDTO>>>
{
    private readonly IOwnersRepository ownersRepository = ownersRepository;

    public async Task<Result<IReadOnlyList<OwnerDTO>>> Handle(GetOwnerQuery request, CancellationToken cancellationToken)
    {
    
        return await ownersRepository.GetAllAsync(cancellationToken)
            .MapAsync(owners => (IReadOnlyList<OwnerDTO>)owners.Select(owner =>
            {
                var unit = owner.UnitOwner?.Unit;
                return new OwnerDTO(
                    Id: owner.Id.ToString(),
                    UnitNumber: unit?.UnitNumber ?? string.Empty,
                    Name: owner.Name,
                    Address: unit?.Address ?? string.Empty,
                    Email: owner.Email?.ToString() ?? string.Empty,
                    PhoneNumber: owner.Phone?.ToString() ?? string.Empty
                );
            }).ToList().AsReadOnly());
    
    }
}
