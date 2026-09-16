using CondoScope.Application.Units;
using CondoScope.Domain.Enums;

namespace CondoScope.Application.FeeCharges;

public record FeeChargeDto(string Id, DateOnly DueDate, string Description, decimal Amount, ChargeCategory Category, ChargeScope Scope, IEnumerable<UnitDto> Units);
