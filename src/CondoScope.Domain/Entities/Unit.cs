using CondoScope.Domain.Common;
using CondoScope.Domain.Errors;
using FluentResults;

namespace CondoScope.Domain.Entities;

public class Unit : BaseAuditableEntity
{
    private Unit(string unitNumber, string? address, bool isActive, DateTime createdAt, string createdBy)
        : base(Ulid.NewUlid(), createdAt, createdBy)
    {
        UnitNumber = unitNumber;
        Address = address;
        IsActive = isActive;
    }

    public string UnitNumber { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }

    private readonly SortedList<DateOnly, UnitOwner> unitOwners = [];
    public IReadOnlyCollection<UnitOwner> UnitOwners => unitOwners.Values.AsReadOnly();

    public static Result<Unit> Create(string unitNumber, string? address, bool isActive, string createdBy)
    {
        return new Unit(unitNumber, address, isActive, DateTime.UtcNow, createdBy);
    }

    public Result AssignOwner(Owner owner, DateOnly effectiveDate, string assignedBy)
    {
        if (unitOwners.Count > 0)
        {
            var lastOwner = unitOwners.Values[^1];
            if (lastOwner.EffectiveFrom > effectiveDate)
                return Result.Fail(new InvalidEffectiveDate(effectiveDate));

            if (lastOwner.EffectiveTo is null)
            {
                lastOwner.EffectiveTo = effectiveDate;
                lastOwner.LastModifiedBy = assignedBy;
                lastOwner.LastModifiedAtUtc = DateTime.UtcNow;
            }
        }

        var unitOwner = new UnitOwner(this, owner, effectiveDate, null, DateTime.UtcNow, assignedBy);
        unitOwners.Add(effectiveDate, unitOwner);

        return Result.Ok();
    }
}
