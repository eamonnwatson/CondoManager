using FluentResults;
using MediatR;

namespace CondoScope.Application.Payments.Queries.GetPayments;

public record GetPaymentsQuery : IRequest<Result<IEnumerable<PaymentDto>>>;