using FluentResults;
using MediatR;

namespace CondoScope.Application.Owners.Commands.CreateOwner;

public record CreateOwnerCommand(string Name, string? EmailAddress, string? PhoneNumber, string? UnitId, DateOnly? EffectiveDate) : IRequest<Result<OwnerDto>>;
