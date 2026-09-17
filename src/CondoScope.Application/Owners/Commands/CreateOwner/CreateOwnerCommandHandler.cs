using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using CondoScope.Domain.Entities;
using CondoScope.Domain.ValueObjects;
using FluentResults;
using FluentResults.Extensions;
using MediatR;

namespace CondoScope.Application.Owners.Commands.CreateOwner;

internal class CreateOwnerCommandHandler(IOwnersRepository ownersRepository, IUnitsRepository unitsRepository, IMapper mapper) : IRequestHandler<CreateOwnerCommand, Result<OwnerDto>>
{
    public async Task<Result<OwnerDto>> Handle(CreateOwnerCommand request, CancellationToken cancellationToken) =>

        await CreateContactInfo(request)
            .Bind(contact => Owner.Create(request.Name, contact.Email, contact.Phone, "System"))
            .Bind(owner => AssignToUnitIfProvidedAsync(owner, request, cancellationToken))
            .Bind(owner => ownersRepository.AddAsync(owner, cancellationToken))
            .Map(mapper.Map<OwnerDto>);


    private static Result<(Email? Email, PhoneNumber? Phone)> CreateContactInfo(CreateOwnerCommand request)
    {
        var emailResult = request.EmailAddress is null
            ? Result.Ok<Email?>(null)
            : Email.Create(request.EmailAddress).Map(email => (Email?)email);

        var phoneResult = request.PhoneNumber is null
            ? Result.Ok<PhoneNumber?>(null)
            : PhoneNumber.Create(request.PhoneNumber).Map(phone => (PhoneNumber?)phone);

        return Result.Merge(emailResult, phoneResult)
            .Bind(() => Result.Ok((emailResult.Value, phoneResult.Value)));
    }

    private async Task<Result<Owner>> AssignToUnitIfProvidedAsync(Owner owner, CreateOwnerCommand request, CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(request.UnitId))
            return owner;

        if (!Ulid.TryParse(request.UnitId, out var unitId))
            return Result.Fail<Owner>("Invalid Unit Id.");

        return await unitsRepository.GetByIdWithDetailsAsync(unitId, cancellationToken)
            .Bind(unit => {
                var assignResult = unit.AssignOwner(owner, request.EffectiveDate ?? DateOnly.MinValue, "System");

                return assignResult.IsFailed
                    ? Result.Fail(assignResult.Errors)
                    : Result.Ok(owner);
            });
    }

}
