using CondoScope.Application.Common.Mapping;
using CondoScope.Domain.Entities;

namespace CondoScope.Application.Ledger;

internal class LedgerMapper : IMapModule
{
    public void RegisterMaps(IMapper mapper)
    {
        mapper.Register<Unit, LedgerDTO>(MapUnitDetailsToLedger);
    }

    private LedgerDTO MapUnitDetailsToLedger(Unit unit)
    {
        var fees = unit.FeeCharges.Where(fc => fc.DueDate <= DateOnly.FromDateTime(DateTime.Today));
        var condoFees = fees.Where(fc => fc.Category == Domain.Enums.ChargeCategory.CondoFee).Sum(fc => fc.Amount);
        var reserveFund = fees.Where(fc => fc.Category == Domain.Enums.ChargeCategory.ReserveFee).Sum(fc => fc.Amount);
        var otherCharges = fees.Where(fc => fc.Category == Domain.Enums.ChargeCategory.OtherFee).Sum(fc => fc.Amount);
        var payments = unit.Payments.Sum(p => p.Amount);
        var balance = (condoFees + reserveFund + otherCharges) - payments;
        var status = balance < 0 ? AccountStatus.Credit : balance > 0 ? AccountStatus.Outstanding : AccountStatus.PaidInFull;

        return new LedgerDTO(
            UnitNumber: unit.UnitNumber,
            OwnerName: unit.CurrentOwner?.Name ?? "N/A",
            AccountStatus: status,
            CondoFees: condoFees,
            ReserveFund: reserveFund,
            OtherCharges: otherCharges,
            Payments: payments,
            Balance: balance
        );
    }
}
