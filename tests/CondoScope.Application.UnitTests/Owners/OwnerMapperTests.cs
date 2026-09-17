using System.Collections;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Owners;
using CondoScope.Domain.Entities;
using CondoScope.Domain.ValueObjects;

namespace CondoScope.Application.UnitTests.Owners;

[TestClass]
public class OwnerMapperTests
{
    /// <summary>
    /// IMapper is an internal interface. Moq's proxy generator cannot create a proxy for it
    /// without an additional InternalsVisibleTo to the dynamic proxy assembly, so a lightweight
    /// fake is used instead to capture and invoke the registered mapping function.
    /// </summary>
    private sealed class FakeMapper : IMapper
    {
        public int RegisterCallCount { get; private set; }

        public Func<Owner, OwnerDto>? RegisteredOwnerMapFunction { get; private set; }

        public TDestination Map<TDestination>(object source) => throw new NotImplementedException();

        public IEnumerable<TDestination> Map<TDestination>(IEnumerable source) => throw new NotImplementedException();

        public IMapper Register<TSource, TDestination>(Func<TSource, TDestination> mapFunction)
        {
            RegisterCallCount++;
            if (mapFunction is Func<Owner, OwnerDto> ownerMapFunction)
                RegisteredOwnerMapFunction = ownerMapFunction;

            return this;
        }
    }

    private static Owner CreateOwner(Email? email = null, PhoneNumber? phone = null)
    {
        var result = Owner.Create("John Doe", email, phone, "tester");
        return result.Value;
    }

    private static Unit CreateUnit(string unitNumber = "101", string address = "123 Main St")
    {
        var result = Unit.Create(unitNumber, address, true, "tester");
        return result.Value;
    }

    [TestMethod]
    public void RegisterMaps_WhenCalled_RegistersOwnerToOwnerDtoMapping()
    {
        // Arrange
        var fakeMapper = new FakeMapper();
        var sut = new OwnerMapper();

        // Act
        sut.RegisterMaps(fakeMapper);

        // Assert
        Assert.AreEqual(1, fakeMapper.RegisterCallCount);
        Assert.IsNotNull(fakeMapper.RegisteredOwnerMapFunction);
    }

    [TestMethod]
    public void RegisterMaps_MapFunctionWithFullyAssignedUnitEmailAndPhone_MapsAllFieldsFromEntities()
    {
        // Arrange
        var email = Email.Create("john@example.com").Value;
        var phone = PhoneNumber.Create("+15551234567").Value;
        var owner = CreateOwner(email, phone);
        var unit = CreateUnit("205", "456 Oak Ave");
        unit.AssignOwner(owner, DateOnly.FromDateTime(DateTime.UtcNow), "tester");

        var fakeMapper = new FakeMapper();
        var sut = new OwnerMapper();
        sut.RegisterMaps(fakeMapper);

        // Act
        var dto = fakeMapper.RegisteredOwnerMapFunction!(owner);

        // Assert
        Assert.AreEqual(owner.Id.ToString(), dto.Id);
        Assert.AreEqual("205", dto.UnitNumber);
        Assert.AreEqual("John Doe", dto.Name);
        Assert.AreEqual("456 Oak Ave", dto.Address);
        Assert.AreEqual("john@example.com", dto.Email);
        Assert.AreEqual("+15551234567", dto.PhoneNumber);
    }

    [TestMethod]
    public void RegisterMaps_MapFunctionWithNoUnitOwnerNoEmailNoPhone_UsesEmptyStringDefaults()
    {
        // Arrange
        var owner = CreateOwner(email: null, phone: null);

        var fakeMapper = new FakeMapper();
        var sut = new OwnerMapper();
        sut.RegisterMaps(fakeMapper);

        // Act
        var dto = fakeMapper.RegisteredOwnerMapFunction!(owner);

        // Assert
        Assert.AreEqual(owner.Id.ToString(), dto.Id);
        Assert.AreEqual(string.Empty, dto.UnitNumber);
        Assert.AreEqual("John Doe", dto.Name);
        Assert.AreEqual(string.Empty, dto.Address);
        Assert.AreEqual(string.Empty, dto.Email);
        Assert.AreEqual(string.Empty, dto.PhoneNumber);
    }

    [TestMethod]
    public void RegisterMaps_MapFunctionWithUnitOwnerButNoEmailOrPhone_MapsUnitFieldsAndEmptyContactInfo()
    {
        // Arrange
        var owner = CreateOwner(email: null, phone: null);
        var unit = CreateUnit("303", "789 Pine Rd");
        unit.AssignOwner(owner, DateOnly.FromDateTime(DateTime.UtcNow), "tester");

        var fakeMapper = new FakeMapper();
        var sut = new OwnerMapper();
        sut.RegisterMaps(fakeMapper);

        // Act
        var dto = fakeMapper.RegisteredOwnerMapFunction!(owner);

        // Assert
        Assert.AreEqual("303", dto.UnitNumber);
        Assert.AreEqual("789 Pine Rd", dto.Address);
        Assert.AreEqual(string.Empty, dto.Email);
        Assert.AreEqual(string.Empty, dto.PhoneNumber);
    }
}
