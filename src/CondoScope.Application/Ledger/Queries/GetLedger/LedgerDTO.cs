namespace CondoScope.Application.Ledger.Queries.GetLedger;

public record LedgerDTO(string UnitNumber, string OwnerName, AccountStatus AccountStatus, decimal CondoFees, decimal ReserveFund, decimal OtherCharges, decimal Payments, decimal Balance);
