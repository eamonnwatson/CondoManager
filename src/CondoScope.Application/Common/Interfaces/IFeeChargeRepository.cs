using CondoScope.Domain.Entities;
using FluentResults;

namespace CondoScope.Application.Common.Interfaces;

public interface IFeeChargeRepository
{
    Task<Result<FeeCharge>> AddAsync(FeeCharge feeCharge, CancellationToken token);
    Task<Result<IReadOnlyList<FeeCharge>>> GetAllAsync(CancellationToken cancellationToken);
}
