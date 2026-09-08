using CondoManager.Domain.Common;
using CondoManager.Domain.Enums;

namespace CondoManager.Domain.Entities;

public class FeeCharge : BaseAuditableEntity
{
    public DateOnly DueDate { get; set; }
    public required string Description { get; set; }
    public required ChargeCategory Category { get; set; }
    public required ChargeScope Scope { get; set; }
    public Ulid? UnitId { get; set; }
    public Unit? Unit { get; set; }
    public decimal Amount { get; set; }
}
