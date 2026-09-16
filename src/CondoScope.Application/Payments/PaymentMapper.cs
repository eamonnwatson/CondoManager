using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Units;
using CondoScope.Domain.Entities;

namespace CondoScope.Application.Payments;

internal class PaymentMapper : IMapModule
{
    public void RegisterMaps(IMapper mapper)
    {

        mapper.Register<Payment, PaymentDto>(payment => new PaymentDto(
            Id: payment.Id.ToString(),
            Unit: mapper.Map<UnitDto>(payment.Unit),
            PaymentDate: payment.PaymentDate,
            Amount: payment.Amount,
            PaymentMethod: payment.Method,
            Reference: payment.Reference ?? string.Empty,
            Notes: payment.Notes ?? string.Empty));

    }
}
