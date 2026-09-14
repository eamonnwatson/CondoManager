using CondoScope.Domain.Entities;
using FluentResults;

namespace CondoScope.Application.Common.Interfaces;

public interface IFeeChargeRepository
{
    Task<Result<IReadOnlyList<FeeCharge>>> GetAllAsync(CancellationToken cancellationToken);
}
