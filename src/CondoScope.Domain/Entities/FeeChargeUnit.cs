namespace CondoScope.Domain.Entities;

public class FeeChargeUnit
{
    private FeeChargeUnit()
    {
        FeeCharge = null!;
        Unit = null!;
    }

    public Ulid FeeChargeId { get; set; }
    public Ulid UnitId { get; set; }

    public FeeCharge FeeCharge { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
}