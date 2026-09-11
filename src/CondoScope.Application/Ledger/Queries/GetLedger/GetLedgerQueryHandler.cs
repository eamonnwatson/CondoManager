using CondoScope.Application.Common.Interfaces;
using FluentResults;
using MediatR;

namespace CondoScope.Application.Ledger.Queries.GetLedger;

public class GetLedgerQueryHandler(IUnitsRepository unitsRepository) : IRequestHandler<GetLedgerQuery, Result<IReadOnlyList<LedgerDTO>>>
{
    public async Task<Result<IReadOnlyList<LedgerDTO>>> Handle(GetLedgerQuery request, CancellationToken cancellationToken)
    {
        var unitsResult = await unitsRepository.GetAllWithDetailsAsync(cancellationToken);
        if (unitsResult.IsFailed)
            return Result.Fail(unitsResult.Errors);

        if (unitsResult.Value is null || !unitsResult.Value.Any())
            return Result.Ok<IReadOnlyList<LedgerDTO>>([]);

        var units = unitsResult.Value;

        var ledger = units.Select(MapUnitDetailsToLedger).ToList();

        return Result.Ok<IReadOnlyList<LedgerDTO>>(ledger);
    }

    private LedgerDTO MapUnitDetailsToLedger(Domain.Entities.Unit unit)
    {

        var condoFees = unit.FeeCharges.Where(fc => fc.Category == Domain.Enums.ChargeCategory.CondoFee).Sum(fc => fc.Amount);
        var reserveFund = unit.FeeCharges.Where(fc => fc.Category == Domain.Enums.ChargeCategory.ReserveFee).Sum(fc => fc.Amount);
        var otherCharges = unit.FeeCharges.Where(fc => fc.Category == Domain.Enums.ChargeCategory.OtherFee).Sum(fc => fc.Amount);
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
