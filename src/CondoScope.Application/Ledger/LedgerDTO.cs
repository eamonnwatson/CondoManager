namespace CondoScope.Application.Ledger;

public record LedgerDTO(string UnitNumber, string OwnerName, AccountStatus AccountStatus, decimal CondoFees, decimal ReserveFund, decimal OtherCharges, decimal Payments, decimal Balance);
