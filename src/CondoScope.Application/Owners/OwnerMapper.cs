using CondoScope.Application.Common.Mapping;
using CondoScope.Domain.Entities;

namespace CondoScope.Application.Owners;

internal class OwnerMapper : IMapModule
{
    public void RegisterMaps(IMapper mapper)
    {
        mapper.Register<Owner, OwnerDto>(owner =>
        {
            var unit = owner.UnitOwner?.Unit;
            return new OwnerDto(
                Id: owner.Id.ToString(),
                UnitNumber: unit?.UnitNumber ?? string.Empty,
                Name: owner.Name,
                Address: unit?.Address ?? string.Empty,
                Email: owner.Email?.ToString() ?? string.Empty,
                PhoneNumber: owner.Phone?.ToString() ?? string.Empty
            );
        });
    }
}
