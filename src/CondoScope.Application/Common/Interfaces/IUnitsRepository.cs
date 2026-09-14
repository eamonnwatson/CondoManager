using CondoScope.Domain.Entities;
using FluentResults;

namespace CondoScope.Application.Common.Interfaces;

public interface IUnitsRepository
{
    Task<Result<Unit>> AddUnitAsync(Unit unit, CancellationToken token);
    Task<Result<IReadOnlyList<Unit>>> GetAllWithDetailsAsync(CancellationToken token);
    Task<Result<IReadOnlyList<Unit>>> GetAllWithCurrentOwner(CancellationToken token);
}
