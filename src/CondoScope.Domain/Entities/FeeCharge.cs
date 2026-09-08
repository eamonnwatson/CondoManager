using CondoScope.Domain.Common;
using CondoScope.Domain.Enums;
using FluentResults;

namespace CondoScope.Domain.Entities;

public class FeeCharge : BaseAuditableEntity
{
    private FeeCharge(decimal amount, DateOnly dueDate, string description, ChargeCategory category, ChargeScope scope, Unit? unit,
                      DateTime createdAt, string createdBy) : base(Ulid.NewUlid(), createdAt, createdBy)
    {
        DueDate = dueDate;
        Description = description;
        Category = category;
        Scope = scope;
        UnitId = unit?.Id;
        Unit = unit;
        Amount = amount;
    }

    public DateOnly DueDate { get; set; }
    public string Description { get; set; }
    public ChargeCategory Category { get; set; }
    public ChargeScope Scope { get; set; }
    public Ulid? UnitId { get; set; }
    public Unit? Unit { get; set; }
    public decimal Amount { get; set; }

    public static Result<FeeCharge> Create(decimal amount, DateOnly dueDate, string description, ChargeCategory category, ChargeScope scope, Unit? unit,
                                   string createdBy)
    {
        return new FeeCharge(amount, dueDate, description, category, scope, unit, DateTime.UtcNow, createdBy);
    }
}
