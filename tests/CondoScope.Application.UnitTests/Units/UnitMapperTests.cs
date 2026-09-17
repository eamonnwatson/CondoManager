using System.Collections;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Units;
using CondoScope.Domain.Entities;

namespace CondoScope.Application.UnitTests.Units;

[TestClass]
public class UnitMapperTests
{
    [TestMethod]
    public void RegisterMaps_WhenCalled_RegistersUnitToUnitDtoMap()
    {
        // Arrange
        var fakeMapper = new FakeMapper();
        var sut = new UnitMapper();

        // Act
        sut.RegisterMaps(fakeMapper);

        // Assert
        Assert.AreEqual(1, fakeMapper.RegisteredSourceTypes.Count);
        Assert.AreEqual(typeof(Unit), fakeMapper.RegisteredSourceTypes[0]);
        Assert.AreEqual(typeof(UnitDto), fakeMapper.RegisteredDestinationTypes[0]);
        Assert.IsNotNull(fakeMapper.RegisteredMapFunction);
    }

    [TestMethod]
    public void RegisterMaps_MapFunctionWithNoOwnerAssigned_MapsUnitFieldsAndEmptyOwnerInfo()
    {
        // Arrange
        var fakeMapper = new FakeMapper();
        var sut = new UnitMapper();
        var unit = CreateUnit("101", "123 Main St");

        // Act
        sut.RegisterMaps(fakeMapper);
        var mapFunction = (Func<Unit, UnitDto>)fakeMapper.RegisteredMapFunction!;
        var dto = mapFunction(unit);

        // Assert
        Assert.AreEqual(unit.Id.ToString(), dto.UnitId);
        Assert.AreEqual("101", dto.UnitNumber);
        Assert.AreEqual("123 Main St", dto.Address);
        Assert.AreEqual(string.Empty, dto.CurrentOwnerId);
        Assert.AreEqual(string.Empty, dto.CurrentOwnerName);
    }

    [TestMethod]
    public void RegisterMaps_MapFunctionWithOwnerAssigned_MapsOwnerIdAndName()
    {
        // Arrange
        var fakeMapper = new FakeMapper();
        var sut = new UnitMapper();
        var unit = CreateUnit("202", "456 Oak Ave");
        var owner = Owner.Create("Jane Doe", null, null, "tester").Value;
        unit.AssignOwner(owner, DateOnly.FromDateTime(DateTime.UtcNow), "tester");

        // Act
        sut.RegisterMaps(fakeMapper);
        var mapFunction = (Func<Unit, UnitDto>)fakeMapper.RegisteredMapFunction!;
        var dto = mapFunction(unit);

        // Assert
        Assert.AreEqual(unit.Id.ToString(), dto.UnitId);
        Assert.AreEqual("202", dto.UnitNumber);
        Assert.AreEqual("456 Oak Ave", dto.Address);
        Assert.AreEqual(owner.Id.ToString(), dto.CurrentOwnerId);
        Assert.AreEqual("Jane Doe", dto.CurrentOwnerName);
    }

    [TestMethod]
    public void RegisterMaps_MapFunctionWithNullAddress_UsesEmptyStringForAddress()
    {
        // Arrange
        var fakeMapper = new FakeMapper();
        var sut = new UnitMapper();
        var unit = CreateUnit("303", "789 Pine Rd");
        unit.Address = null!;

        // Act
        sut.RegisterMaps(fakeMapper);
        var mapFunction = (Func<Unit, UnitDto>)fakeMapper.RegisteredMapFunction!;
        var dto = mapFunction(unit);

        // Assert
        Assert.AreEqual(string.Empty, dto.Address);
    }

    private static Unit CreateUnit(string unitNumber, string address)
    {
        var result = Unit.Create(unitNumber, address, true, "tester");
        return result.Value;
    }

    private sealed class FakeMapper : IMapper
    {
        public List<Type> RegisteredSourceTypes { get; } = [];

        public List<Type> RegisteredDestinationTypes { get; } = [];

        public Delegate? RegisteredMapFunction { get; private set; }

        public TDestination Map<TDestination>(object source) => throw new NotSupportedException();

        public IEnumerable<TDestination> Map<TDestination>(IEnumerable source) => throw new NotSupportedException();

        public IMapper Register<TSource, TDestination>(Func<TSource, TDestination> mapFunction)
        {
            this.RegisteredSourceTypes.Add(typeof(TSource));
            this.RegisteredDestinationTypes.Add(typeof(TDestination));
            this.RegisteredMapFunction = mapFunction;
            return this;
        }
    }
}
