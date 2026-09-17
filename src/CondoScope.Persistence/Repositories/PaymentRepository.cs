using CondoScope.Application.Common.Interfaces;
using CondoScope.Domain.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace CondoScope.Persistence.Repositories;

internal class PaymentRepository(AppDbContext dbContext) : BaseRepository, IPaymentRepository
{
    public Task<Result<Payment>> AddAsync(Payment payment, CancellationToken token) =>
        ExecuteAsync(async () =>
        {
            await dbContext.AddAsync(payment, token);
            await dbContext.SaveChangesAsync(token);
            return payment;
        });

    public Task<Result<IReadOnlyList<Payment>>> GetAllAsync(CancellationToken cancellationToken) =>
        ExecuteAsync(async () => (IReadOnlyList<Payment>)await dbContext.Payments
            .Include(p => p.Unit)
            .ToListAsync(cancellationToken));
}
