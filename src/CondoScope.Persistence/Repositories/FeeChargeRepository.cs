using CondoScope.Application.Common.Interfaces;
using CondoScope.Domain.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace CondoScope.Persistence.Repositories;

internal class FeeChargeRepository(AppDbContext dbContext) : BaseRepository, IFeeChargeRepository
{
    public Task<Result<IReadOnlyList<FeeCharge>>> GetAllAsync(CancellationToken cancellationToken) =>
        ExecuteAsync(async () => (IReadOnlyList<FeeCharge>)await dbContext.FeeCharges
            .Include(fc => fc.Units)
            .OrderByDescending(fc => fc.DueDate)
            .ToListAsync(cancellationToken));
}
