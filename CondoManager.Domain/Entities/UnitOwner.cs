using CondoManager.Domain.Common;

namespace CondoManager.Domain.Entities;

public class UnitOwner : BaseAuditableEntity
{
    protected UnitOwner(Unit unit, Owner owner, DateOnly effectiveFrom, DateOnly? effectiveTo, DateTime createdAt, string createdBy)
        : base(Ulid.NewUlid(), createdAt, createdBy)
    {
        Unit = unit;
        Owner = owner;
        UnitId = unit.Id;
        OwnerId = owner.Id;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }
    public Ulid UnitId { get; set; }
    public Ulid OwnerId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public Unit Unit { get; set; } = null!;
    public Owner Owner { get; set; } = null!;
}
