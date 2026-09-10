using CondoScope.Application.Common.Errors;
using CondoScope.Application.Common.Interfaces;
using CondoScope.Domain.Entities;
using FluentResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CondoScope.Persistence.Repositories;

internal class OwnerRepository(AppDbContext dbContext) : IOwnersRepository
{
    private readonly AppDbContext dbContext = dbContext;

    public async Task<Result<IReadOnlyList<Owner>>> GetAllAsync(CancellationToken token)
    { 
        try
        {
            return await dbContext.Owners
                .Include(o => o.UnitOwner)
                .ThenInclude(uo => uo!.Unit)
                .ToListAsync(token);
        }
        catch (SqliteException ex)
        {
            return Result.Fail(new DatabaseError(ex));
        }
        catch (Exception ex)
        {
            return Result.Fail(new UnexpectedAppError(ex));
        }
    }
}
