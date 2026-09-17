using CondoScope.Domain.Entities;
using FluentResults;

namespace CondoScope.Application.Common.Interfaces;

public interface IUnitsRepository
{
    Task<Result<Unit>> AddAsync(Unit unit, CancellationToken token);
    Task<Result<IReadOnlyList<Unit>>> GetAllWithDetailsAsync(CancellationToken token);
    Task<Result<IReadOnlyList<Unit>>> GetAllWithCurrentOwnerAsync(CancellationToken token);
    Task<Result<Unit>> GetByIdWithDetailsAsync(Ulid unitId, CancellationToken token);
    Task<Result<IReadOnlyList<Unit>>> GetUnitsWithNoOwnersAsync(CancellationToken token);
}
