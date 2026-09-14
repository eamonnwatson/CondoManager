using FluentResults;
using MediatR;

namespace CondoScope.Application.FeeCharges.Queries.GetFeeCharges;

public record GetFeeChargesQuery : IRequest<Result<IReadOnlyList<FeeChargeDto>>>;