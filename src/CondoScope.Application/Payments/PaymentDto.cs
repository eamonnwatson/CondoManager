using CondoScope.Application.Units;
using CondoScope.Domain.Enums;

namespace CondoScope.Application.Payments;

public record PaymentDto(string Id, UnitDto Unit, DateOnly PaymentDate, decimal Amount, PaymentMethod PaymentMethod, string Reference, string Notes);