using CondoScope.Domain.Entities;
using FluentResults;

namespace CondoScope.Application.Common.Interfaces;

public interface IUnitsRepository
{
    Task<Result<IReadOnlyList<Unit>>> GetAllWithDetailsAsync(CancellationToken token);
}
