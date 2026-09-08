using System.ComponentModel.DataAnnotations;

namespace CondoScope.Domain.Common;

public abstract class BaseAuditableEntity(Ulid id, DateTime createdAt, string createdBy) : IEquatable<BaseAuditableEntity>
{
    public Ulid Id { get; init; } = id;
    public DateTime CreatedAtUtc { get; init; } = createdAt;
    public string CreatedBy { get; init; } = createdBy;
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public bool Equals(BaseAuditableEntity? other)
        => other is not null && Id == other.Id;

    public override bool Equals(object? obj)
        => obj is BaseAuditableEntity entity && Equals(entity);

    public override int GetHashCode()
        => Id.GetHashCode();

}

