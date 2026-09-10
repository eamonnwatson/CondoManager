using CondoScope.Domain.Common;
using CondoScope.Domain.Enums;
using FluentResults;

namespace CondoScope.Domain.Entities;

public class FeeCharge : BaseAuditableEntity
{
    private readonly List<Unit> units = [];

    private FeeCharge()
    {
        Description = null!;
    }

    private FeeCharge(decimal amount, DateOnly dueDate, string description, ChargeCategory category, ChargeScope scope,
                      DateTime createdAt, string createdBy) : base(Ulid.NewUlid(), createdAt, createdBy)
    {
        DueDate = dueDate;
        Description = description;
        Category = category;
        Scope = scope;
        Amount = amount;
    }

    public DateOnly DueDate { get; set; }
    public string Description { get; set; }
    public ChargeCategory Category { get; set; }
    public ChargeScope Scope { get; set; }
    public decimal Amount { get; set; }
    public IReadOnlyCollection<Unit> Units => units.AsReadOnly();

    public static Result<FeeCharge> Create(decimal amount, DateOnly dueDate, string description, ChargeCategory category, ChargeScope scope,
        Unit? unit, string createdBy, IReadOnlyCollection<Unit>? allUnits = null)
    {
        var feeCharge = new FeeCharge(amount, dueDate, description, category, scope, DateTime.UtcNow, createdBy);

        if (scope == ChargeScope.SpecificUnit)
        {
            if (unit is null)
                return Result.Fail("A specific unit fee charge must have an assigned unit.");

            feeCharge.units.Add(unit);
            return feeCharge;
        }

        if (unit is not null)
            return Result.Fail("An all units fee charge cannot be created with a single assigned unit.");

        if (allUnits is null || allUnits.Count == 0)
            return Result.Fail("An all units fee charge must be assigned to at least one unit.");

        foreach (var relatedUnit in allUnits.DistinctBy(u => u.Id))
            feeCharge.units.Add(relatedUnit);

        return feeCharge;
    }
}
