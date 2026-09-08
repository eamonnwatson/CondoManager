using CondoManager.Domain.Common;
using CondoManager.Domain.Enums;

namespace CondoManager.Domain.Entities;

public class Payment : BaseAuditableEntity
{
    public required DateOnly PaymentDate { get; set; }
    public required Ulid UnitId { get; set; }
    public required decimal Amount { get; set; }
    public required PaymentMethod Method { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public Unit Unit { get; set; } = null!;
}
