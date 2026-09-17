using CondoScope.Domain.Common;

namespace CondoScope.Domain.UnitTests.Common;

[TestClass]
public class BaseAuditableEntityTests
{
    private sealed class TestEntity(Ulid id, DateTime createdAt, string createdBy)
        : BaseAuditableEntity(id, createdAt, createdBy);

    private sealed class ParameterlessTestEntity : BaseAuditableEntity;

    [TestMethod]
    public void Constructor_Parameterless_SetsDefaultValues()
    {
        var entity = new ParameterlessTestEntity();

        Assert.AreEqual(default(Ulid), entity.Id);
        Assert.AreEqual(default(DateTime), entity.CreatedAtUtc);
        Assert.IsNull(entity.CreatedBy);
    }

    [TestMethod]
    public void Equals_OtherIsNull_ReturnsFalse()
    {
        var entity = new TestEntity(Ulid.NewUlid(), DateTime.UtcNow, "user");

        var result = entity.Equals(null);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equals_SameId_ReturnsTrue()
    {
        var id = Ulid.NewUlid();
        var entity1 = new TestEntity(id, DateTime.UtcNow, "user");
        var entity2 = new TestEntity(id, DateTime.UtcNow.AddDays(1), "otherUser");

        var result = entity1.Equals(entity2);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var entity1 = new TestEntity(Ulid.NewUlid(), DateTime.UtcNow, "user");
        var entity2 = new TestEntity(Ulid.NewUlid(), DateTime.UtcNow, "user");

        var result = entity1.Equals(entity2);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equals_ObjIsNull_ReturnsFalse()
    {
        var entity = new TestEntity(Ulid.NewUlid(), DateTime.UtcNow, "user");

        var result = entity.Equals((object?)null);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equals_ObjIsNotBaseAuditableEntity_ReturnsFalse()
    {
        var entity = new TestEntity(Ulid.NewUlid(), DateTime.UtcNow, "user");

        var result = entity.Equals("not an entity");

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equals_ObjIsSameIdEntity_ReturnsTrue()
    {
        var id = Ulid.NewUlid();
        var entity1 = new TestEntity(id, DateTime.UtcNow, "user");
        object entity2 = new TestEntity(id, DateTime.UtcNow, "user");

        var result = entity1.Equals(entity2);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Equals_ObjIsDifferentIdEntity_ReturnsFalse()
    {
        var entity1 = new TestEntity(Ulid.NewUlid(), DateTime.UtcNow, "user");
        object entity2 = new TestEntity(Ulid.NewUlid(), DateTime.UtcNow, "user");

        var result = entity1.Equals(entity2);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetHashCode_SameId_ReturnsSameHashCode()
    {
        var id = Ulid.NewUlid();
        var entity1 = new TestEntity(id, DateTime.UtcNow, "user");
        var entity2 = new TestEntity(id, DateTime.UtcNow.AddDays(1), "otherUser");

        Assert.AreEqual(entity1.GetHashCode(), entity2.GetHashCode());
    }

    [TestMethod]
    public void GetHashCode_MatchesIdHashCode()
    {
        var id = Ulid.NewUlid();
        var entity = new TestEntity(id, DateTime.UtcNow, "user");

        Assert.AreEqual(id.GetHashCode(), entity.GetHashCode());
    }
}
