using CondoScope.Domain.Entities;
using FluentResults;

namespace CondoScope.Application.Common.Interfaces;

public interface IOwnersRepository
{
    Task<Result<Owner>> AddAsync(Owner owner, CancellationToken token);
    Task<Result<IReadOnlyList<Owner>>> GetAllAsync(CancellationToken token);
}
