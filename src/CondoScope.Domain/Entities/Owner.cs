using CondoScope.Domain.Common;
using CondoScope.Domain.ValueObjects;
using FluentResults;

namespace CondoScope.Domain.Entities;

public class Owner : BaseAuditableEntity
{
    private Owner()
    {
        Name = null!;
    }

    private Owner(string name, Email? email, PhoneNumber? phone, DateTime createdAt, string createdBy)
        : base(Ulid.NewUlid(), createdAt, createdBy)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }

    public string Name { get; set; }
    public Email? Email { get; set; }
    public PhoneNumber? Phone { get; set; }

    public UnitOwner? UnitOwner { get; set; }

    internal void SetUnitOwner(UnitOwner unitOwner) => UnitOwner = unitOwner;

    public static Result<Owner> Create(string name, Email? email, PhoneNumber? phone, string createdBy)
    {
        return new Owner(name, email, phone, DateTime.UtcNow, createdBy);
    }
}
