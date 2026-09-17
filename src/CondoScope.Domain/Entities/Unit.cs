using CondoScope.Domain.Common;
using CondoScope.Domain.Errors;
using FluentResults;

namespace CondoScope.Domain.Entities;

public class Unit : BaseAuditableEntity
{
    private Unit()
    {
        UnitNumber = null!;
        Address = null!;
    }

    private Unit(string unitNumber, string address, bool isActive, DateTime createdAt, string createdBy)
        : base(Ulid.NewUlid(), createdAt, createdBy)
    {
        UnitNumber = unitNumber;
        Address = address;
        IsActive = isActive;
    }

    public string UnitNumber { get; set; }
    public string Address { get; set; }
    public bool IsActive { get; set; }

    private readonly List<UnitOwner> unitOwners = [];
    public IReadOnlyCollection<UnitOwner> UnitOwners => unitOwners.AsReadOnly();
    public Owner? CurrentOwner => unitOwners.LastOrDefault()?.Owner;

    private readonly List<Payment> payments = [];
    public IReadOnlyCollection<Payment> Payments => payments.AsReadOnly();

    private readonly List<FeeCharge> feeCharges = [];
    public IReadOnlyCollection<FeeCharge> FeeCharges => feeCharges.AsReadOnly();

    public static Result<Unit> Create(string unitNumber, string address, bool isActive, string createdBy)
    {
        return new Unit(unitNumber, address, isActive, DateTime.UtcNow, createdBy);
    }

    public Result AssignOwner(Owner owner, DateOnly effectiveDate, string assignedBy)
    {
        if (owner.UnitOwner is not null && owner.UnitOwner.UnitId != Id)
            return Result.Fail(new OwnerAlreadyAssigned(owner.Id, owner.UnitOwner.UnitId));

        if (unitOwners.Count > 0)
        {
            var lastOwner = unitOwners[^1];
            if (lastOwner.EffectiveFrom >= effectiveDate)
                return Result.Fail(new InvalidEffectiveDate(effectiveDate));

            if (lastOwner.EffectiveTo is null)
            {
                lastOwner.EffectiveTo = effectiveDate;
                lastOwner.LastModifiedBy = assignedBy;
                lastOwner.LastModifiedAtUtc = DateTime.UtcNow;
            }
        }

        var unitOwner = new UnitOwner(this, owner, effectiveDate, null, DateTime.UtcNow, assignedBy);
        unitOwners.Add(unitOwner);
        owner.SetUnitOwner(unitOwner);

        return Result.Ok();
    }
}
