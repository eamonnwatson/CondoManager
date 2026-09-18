using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Ledger;
using CondoScope.Domain.Entities;

namespace CondoScope.Application.Statement;

internal class StatementMapper : IMapModule
{
    public void RegisterMaps(IMapper mapper)
    {
        mapper.Register<Payment, AccountActivityDto>(payment => new AccountActivityDto(
            ActivityDate: payment.PaymentDate,
            Description: "Payment Received",
            Charge: null,
            Payment: payment.Amount,
            Balance: 0
        ));

        mapper.Register<FeeCharge, AccountActivityDto>(feeCharge => new AccountActivityDto(
            ActivityDate: feeCharge.DueDate,
            Description: feeCharge.Description,
            Charge: feeCharge.Amount,
            Payment: null,
            Balance: 0
        ));

        mapper.Register<Unit, StatementDto>(unit =>
        {
            var payments = mapper.Map<AccountActivityDto>(unit.Payments);
            var feeCharges = mapper.Map<AccountActivityDto>(unit.FeeCharges);

            var today = DateOnly.FromDateTime(DateTime.Today);

            var balance = 0m;
            var accountActivitiesWithBalance = payments.Concat(feeCharges)
                .Where(activity => activity.ActivityDate <= today)
                .OrderBy(activity => activity.ActivityDate)
                .ThenBy(activity => activity.Payment.HasValue)
                .Select(activity =>
                {
                    balance += (activity.Charge ?? 0) - (activity.Payment ?? 0);
                    return activity with { Balance = balance };
                })
                .ToList();

            return new StatementDto(
                UnitNumber: unit.UnitNumber,
                OwnerName: unit.CurrentOwner?.Name ?? string.Empty,
                Address: unit.Address,
                EmailAddress: unit.CurrentOwner?.Email?.Value ?? string.Empty,
                PhoneNumber: unit.CurrentOwner?.Phone?.Value ?? string.Empty,
                CurrentBalance: balance,
                AccountStatus: balance < 0 ? AccountStatus.Credit : balance > 0 ? AccountStatus.Outstanding : AccountStatus.PaidInFull,
                AccountActivities: accountActivitiesWithBalance
            );
        });

    }
}
