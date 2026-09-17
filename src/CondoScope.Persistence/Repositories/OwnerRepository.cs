using CondoScope.Application.Common.Interfaces;
using CondoScope.Domain.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace CondoScope.Persistence.Repositories;

internal class OwnerRepository(AppDbContext dbContext) : BaseRepository, IOwnersRepository
{
    public Task<Result<Owner>> AddAsync(Owner owner, CancellationToken token) =>
        ExecuteAsync(async () =>
        {
            await dbContext.AddAsync(owner, token);
            await dbContext.SaveChangesAsync(token);
            return owner;
        });

    public Task<Result<IReadOnlyList<Owner>>> GetAllAsync(CancellationToken token) =>
        ExecuteAsync(async () => (IReadOnlyList<Owner>)await dbContext.Owners
            .Include(o => o.UnitOwner)
            .ThenInclude(uo => uo!.Unit)
            .ToListAsync(token));

}
