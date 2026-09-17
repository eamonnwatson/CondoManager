using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Units;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;

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

        mapper.Register<PaymentMethod, string>(method => method switch
            {
                PaymentMethod.Cash => "Cash",
                PaymentMethod.BankDraft => "Bank Draft",
                PaymentMethod.Cheque => "Cheque",
                PaymentMethod.ETransfer => "E-Transfer",
                PaymentMethod.Other => "Other",
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
            });
    }
}
