using CondoManager.Domain.Common;
using FluentResults;

namespace CondoManager.Domain.Entities;

public class Unit : BaseAuditableEntity
{
    private Unit(Ulid id, string unitNumber, string? address, bool isActive, DateTime createdAt, string createdBy)
        : base(id, createdAt, createdBy)
    {
        UnitNumber = unitNumber;
        Address = address;
        IsActive = isActive;
    }

    public string UnitNumber { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }

    private readonly List<UnitOwner> unitOwners = [];
    public IReadOnlyCollection<UnitOwner> UnitOwners => unitOwners.AsReadOnly();

    public static Result<Unit> Create(string unitNumber, string? address, bool isActive, string createdBy)
    {
        var id = Ulid.NewUlid();
        var createdAt = DateTime.UtcNow;
        var unit = new Unit(id, unitNumber, address, isActive, createdAt, createdBy);
        return unit;
    }

}
