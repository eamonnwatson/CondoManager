using CondoScope.Application.Common.Interfaces;
using CondoScope.Domain.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace CondoScope.Persistence.Repositories;

internal class UnitRepository(AppDbContext dbContext) : BaseRepository, IUnitsRepository
{
    public Task<Result<Unit>> AddUnitAsync(Unit unit, CancellationToken token) =>
        ExecuteAsync(async () =>
        {
            await dbContext.AddAsync(unit, token);
            await dbContext.SaveChangesAsync(token);
            return unit;
        });

    public Task<Result<IReadOnlyList<Unit>>> GetAllWithCurrentOwner(CancellationToken token) =>
        ExecuteAsync(async () => (IReadOnlyList<Unit>)await dbContext.Units
            .Include(u => u.UnitOwners.Where(uo => uo.EffectiveTo == null))
                .ThenInclude(uo => uo.Owner)
            .OrderBy(u => u.UnitNumber)
            .ToListAsync(token));
    public Task<Result<IReadOnlyList<Unit>>> GetAllWithDetailsAsync(CancellationToken token) =>
        ExecuteAsync(async () => (IReadOnlyList<Unit>)await dbContext.Units
            .Include(u => u.UnitOwners)
                .ThenInclude(uo => uo.Owner)
            .Include(u => u.Payments)
            .Include(u => u.FeeCharges)
            .OrderBy(u => u.UnitNumber)
            .ToListAsync(token));

    public Task<Result<Unit>> GetByIdWithDetailsAsync(Ulid unitId, CancellationToken token) =>
        GetDataAsync(async () => await dbContext.Units
                .Include(u => u.UnitOwners)
                    .ThenInclude(uo => uo.Owner)
                .Include(u => u.Payments)
                .Include(u => u.FeeCharges)
                .FirstOrDefaultAsync(u => u.Id == unitId, token), $"Unit with ID {unitId} not found.");

}
