using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using FluentResults;
using FluentResults.Extensions;
using MediatR;

namespace CondoScope.Application.Payments.Queries.GetPayments;

internal class GetPaymentsQueryHandler(IPaymentRepository paymentRepository, IMapper mapper) : IRequestHandler<GetPaymentsQuery, Result<IEnumerable<PaymentDto>>>
{
    public async Task<Result<IEnumerable<PaymentDto>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken) =>
        await paymentRepository.GetAllAsync(cancellationToken)
            .Map(mapper.Map<PaymentDto>);
}
