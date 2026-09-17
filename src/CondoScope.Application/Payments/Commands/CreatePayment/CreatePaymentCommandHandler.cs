using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using CondoScope.Domain.Entities;
using FluentResults;
using FluentResults.Extensions;
using MediatR;

namespace CondoScope.Application.Payments.Commands.CreatePayment;

internal class CreatePaymentCommandHandler(IPaymentRepository paymentRepository, IUnitsRepository unitsRepository, IMapper mapper) : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken) =>
        await unitsRepository.GetAllWithCurrentOwnerAsync(cancellationToken)
            .Bind(units => Payment.Create(
                paymentDate: request.PaymentDate,
                unit: units.First(u => u.Id == Ulid.Parse(request.UnitId)),
                amount: request.Amount,
                method: request.PaymentMethod,
                reference: request.ReferenceNumber,
                notes: request.Notes,
                "System"))
            .Bind(payment => paymentRepository.AddAsync(payment, cancellationToken))
            .Map(mapper.Map<PaymentDto>);

}
