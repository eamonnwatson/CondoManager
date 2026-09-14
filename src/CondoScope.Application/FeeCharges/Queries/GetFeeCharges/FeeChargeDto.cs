namespace CondoScope.Application.FeeCharges.Queries.GetFeeCharges;

public record FeeChargeDto(string Id, DateOnly DueDate, string Description, string Category, string AppliesTo, decimal Amount);
