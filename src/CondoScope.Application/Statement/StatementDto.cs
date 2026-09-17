using CondoScope.Application.Ledger;

namespace CondoScope.Application.Statement;

public record StatementDto(
    string UnitNumber,
    string OwnerName,
    string Address,
    string EmailAddress,
    string PhoneNumber,
    decimal CurrentBalance,
    AccountStatus AccountStatus,
    IEnumerable<AccountActivityDto> AccountActivities);

