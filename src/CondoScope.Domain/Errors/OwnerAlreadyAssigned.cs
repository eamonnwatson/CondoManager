using FluentResults;

namespace CondoScope.Domain.Errors;

public class OwnerAlreadyAssigned : Error
{
    public Ulid OwnerId { get; }
    public Ulid ExistingUnitId { get; }

    public OwnerAlreadyAssigned(Ulid ownerId, Ulid existingUnitId)
        : base($"Owner '{ownerId}' is already assigned to unit '{existingUnitId}' and cannot be assigned to another unit.")
    {
        OwnerId = ownerId;
        ExistingUnitId = existingUnitId;

        WithMetadata("ErrorCode", "DOM_OWNER_ALREADY_ASSIGNED");
        WithMetadata("TimeStamp", DateTime.UtcNow);
    }
}
