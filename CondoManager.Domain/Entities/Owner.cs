using CondoManager.Domain.Common;
using CondoManager.Domain.ValueObjects;
using FluentResults;

namespace CondoManager.Domain.Entities;

public class Owner : BaseAuditableEntity
{
    private Owner(Ulid id, string name, Email? email, PhoneNumber? phone, DateTime createdAt, string createdBy)
        : base(id, createdAt, createdBy)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }

    public string Name { get; set; }
    public Email? Email { get; set; }
    public PhoneNumber? Phone { get; set; }
    private readonly List<UnitOwner> unitOwners = [];
    public IReadOnlyCollection<UnitOwner> UnitOwners => unitOwners.AsReadOnly();

    public static Result<Owner> Create(string name, Email? email, PhoneNumber? phone, string createdBy)
    {
        var id = Ulid.NewUlid();
        var createdAt = DateTime.UtcNow;
        var owner = new Owner(id, name, email, phone, createdAt, createdBy);
        return owner;
    }
}
