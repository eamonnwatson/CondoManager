using CondoScope.Domain.Enums;
using FluentResults;
using MediatR;

namespace CondoScope.Application.FeeCharges.Commands.CreateFeeCharge;

public record CreateFeeChargeCommand(DateOnly DueDate, string Description, decimal Amount, ChargeCategory Category, ChargeScope Scope, string UnitId) : IRequest<Result<FeeChargeDto>>;
