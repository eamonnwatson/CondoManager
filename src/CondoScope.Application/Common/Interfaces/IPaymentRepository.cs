using CondoScope.Domain.Entities;
using FluentResults;

namespace CondoScope.Application.Common.Interfaces;

public interface IPaymentRepository
{
    Task<Result<IReadOnlyList<Payment>>> GetAllAsync(CancellationToken cancellationToken);

}
