using CondoScope.Domain.Entities;
using FluentResults;

namespace CondoScope.Application.Common.Interfaces;

public interface IOwnersRepository
{
    Task<Result<IReadOnlyList<Owner>>> GetAllAsync(CancellationToken token);
}
