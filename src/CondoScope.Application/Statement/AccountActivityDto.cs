namespace CondoScope.Application.Statement;

public record AccountActivityDto(DateOnly ActivityDate, string Description, decimal? Charge, decimal? Payment, decimal Balance);
