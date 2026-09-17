using System.Collections;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Payments;
using CondoScope.Application.Units;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;

namespace CondoScope.Application.UnitTests.Payments;

[TestClass]
public class PaymentMapperTests
{
    /// <summary>
    /// IMapper is an internal interface. Moq's proxy generator cannot create a proxy for it
    /// without an additional InternalsVisibleTo to the dynamic proxy assembly, so a lightweight
    /// fake is used instead to capture and invoke the registered mapping function.
    /// </summary>
    private sealed class FakeMapper : IMapper
    {
        public int RegisterCallCount { get; private set; }

        public Func<Payment, PaymentDto>? RegisteredPaymentMapFunction { get; private set; }

        public UnitDto? NextUnitDtoResult { get; set; }

        public Unit? LastMappedUnit { get; private set; }

        public TDestination Map<TDestination>(object source)
        {
            if (typeof(TDestination) == typeof(UnitDto) && source is Unit unit)
            {
                LastMappedUnit = unit;
                return (TDestination)(object)NextUnitDtoResult!;
            }

            throw new NotSupportedException();
        }

        public IEnumerable<TDestination> Map<TDestination>(IEnumerable source) => throw new NotSupportedException();

        public IMapper Register<TSource, TDestination>(Func<TSource, TDestination> mapFunction)
        {
            RegisterCallCount++;
            if (mapFunction is Func<Payment, PaymentDto> paymentMapFunction)
                RegisteredPaymentMapFunction = paymentMapFunction;

            return this;
        }
    }

    private static Unit CreateUnit(string unitNumber = "101", string address = "123 Main St") =>
        Unit.Create(unitNumber, address, true, "tester").Value;

    private static Payment CreatePayment(Unit unit, string? reference, string? notes, decimal amount = 100m,
        PaymentMethod method = PaymentMethod.Cash) =>
        Payment.Create(new DateOnly(2024, 1, 15), unit, amount, method, reference, notes, "tester").Value;

    [TestMethod]
    public void RegisterMaps_WhenCalled_RegistersPaymentToPaymentDtoMapping()
    {
        // Arrange
        var fakeMapper = new FakeMapper();
        var sut = new PaymentMapper();

        // Act
        sut.RegisterMaps(fakeMapper);

        // Assert
        Assert.AreEqual(1, fakeMapper.RegisterCallCount);
        Assert.IsNotNull(fakeMapper.RegisteredPaymentMapFunction);
    }

    [TestMethod]
    public void RegisterMaps_MapFunctionWithReferenceAndNotes_MapsAllFieldsFromEntity()
    {
        // Arrange
        var unit = CreateUnit("205", "456 Oak Ave");
        var payment = CreatePayment(unit, "REF-123", "Some notes", 250.75m, PaymentMethod.Cheque);

        var fakeMapper = new FakeMapper();
        var expectedUnitDto = new UnitDto(unit.Id.ToString(), "205", "456 Oak Ave", string.Empty, string.Empty);
        fakeMapper.NextUnitDtoResult = expectedUnitDto;

        var sut = new PaymentMapper();
        sut.RegisterMaps(fakeMapper);

        // Act
        var dto = fakeMapper.RegisteredPaymentMapFunction!(payment);

        // Assert
        Assert.AreEqual(payment.Id.ToString(), dto.Id);
        Assert.AreSame(expectedUnitDto, dto.Unit);
        Assert.AreEqual(payment.PaymentDate, dto.PaymentDate);
        Assert.AreEqual(250.75m, dto.Amount);
        Assert.AreEqual(PaymentMethod.Cheque, dto.PaymentMethod);
        Assert.AreEqual("REF-123", dto.Reference);
        Assert.AreEqual("Some notes", dto.Notes);
        Assert.AreSame(unit, fakeMapper.LastMappedUnit);
    }

    [TestMethod]
    public void RegisterMaps_MapFunctionWithNullReferenceAndNotes_UsesEmptyStringDefaults()
    {
        // Arrange
        var unit = CreateUnit();
        var payment = CreatePayment(unit, reference: null, notes: null);

        var fakeMapper = new FakeMapper();
        fakeMapper.NextUnitDtoResult = new UnitDto(unit.Id.ToString(), "101", "123 Main St", string.Empty, string.Empty);

        var sut = new PaymentMapper();
        sut.RegisterMaps(fakeMapper);

        // Act
        var dto = fakeMapper.RegisteredPaymentMapFunction!(payment);

        // Assert
        Assert.AreEqual(string.Empty, dto.Reference);
        Assert.AreEqual(string.Empty, dto.Notes);
    }

    [TestMethod]
    public void RegisterMaps_MapFunctionWithReferenceButNoNotes_MapsReferenceAndEmptyNotes()
    {
        // Arrange
        var unit = CreateUnit();
        var payment = CreatePayment(unit, reference: "REF-999", notes: null);

        var fakeMapper = new FakeMapper();
        fakeMapper.NextUnitDtoResult = new UnitDto(unit.Id.ToString(), "101", "123 Main St", string.Empty, string.Empty);

        var sut = new PaymentMapper();
        sut.RegisterMaps(fakeMapper);

        // Act
        var dto = fakeMapper.RegisteredPaymentMapFunction!(payment);

        // Assert
        Assert.AreEqual("REF-999", dto.Reference);
        Assert.AreEqual(string.Empty, dto.Notes);
    }

    [TestMethod]
    public void RegisterMaps_MapFunctionWithNotesButNoReference_MapsNotesAndEmptyReference()
    {
        // Arrange
        var unit = CreateUnit();
        var payment = CreatePayment(unit, reference: null, notes: "Some notes");

        var fakeMapper = new FakeMapper();
        fakeMapper.NextUnitDtoResult = new UnitDto(unit.Id.ToString(), "101", "123 Main St", string.Empty, string.Empty);

        var sut = new PaymentMapper();
        sut.RegisterMaps(fakeMapper);

        // Act
        var dto = fakeMapper.RegisteredPaymentMapFunction!(payment);

        // Assert
        Assert.AreEqual(string.Empty, dto.Reference);
        Assert.AreEqual("Some notes", dto.Notes);
    }
}
