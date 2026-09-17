using System.Collections;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Ledger;
using CondoScope.Application.Statement;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using CondoScope.Domain.ValueObjects;

namespace CondoScope.Application.UnitTests.Statement;

[TestClass]
public class StatementMapperTests
{
    [TestMethod]
    public void RegisterMaps_WhenCalled_RegistersThreeMaps()
    {
        // Arrange
        var recordingMapper = new RecordingMapper();
        var sut = new StatementMapper();

        // Act
        sut.RegisterMaps(recordingMapper);

        // Assert
        Assert.AreEqual(3, recordingMapper.Registrations.Count);
        Assert.AreEqual((typeof(Payment), typeof(AccountActivityDto)), recordingMapper.Registrations[0]);
        Assert.AreEqual((typeof(FeeCharge), typeof(AccountActivityDto)), recordingMapper.Registrations[1]);
        Assert.AreEqual((typeof(Unit), typeof(StatementDto)), recordingMapper.Registrations[2]);
    }

    [TestMethod]
    public void RegisterMaps_PaymentMapFunction_MapsPaymentToAccountActivityDto()
    {
        // Arrange
        var recordingMapper = new RecordingMapper();
        var sut = new StatementMapper();
        sut.RegisterMaps(recordingMapper);

        var unit = CreateUnit("101", "123 Main St");
        var paymentDate = new DateOnly(2024, 3, 15);
        var payment = Payment.Create(paymentDate, unit, 250.75m, PaymentMethod.ETransfer, "ref", "notes", "tester").Value;

        // Act
        var dto = recordingMapper.Map<AccountActivityDto>(payment);

        // Assert
        Assert.AreEqual(paymentDate, dto.ActivityDate);
        Assert.AreEqual("Payment Received", dto.Description);
        Assert.IsNull(dto.Charge);
        Assert.AreEqual(250.75m, dto.Payment);
        Assert.AreEqual(0m, dto.Balance);
    }

    [TestMethod]
    public void RegisterMaps_FeeChargeMapFunction_MapsFeeChargeToAccountActivityDto()
    {
        // Arrange
        var recordingMapper = new RecordingMapper();
        var sut = new StatementMapper();
        sut.RegisterMaps(recordingMapper);

        var unit = CreateUnit("202", "456 Oak Ave");
        var dueDate = new DateOnly(2024, 4, 1);
        var feeCharge = FeeCharge.Create(100m, dueDate, "Monthly Condo Fee", ChargeCategory.CondoFee, ChargeScope.SpecificUnit, unit, "tester").Value;

        // Act
        var dto = recordingMapper.Map<AccountActivityDto>(feeCharge);

        // Assert
        Assert.AreEqual(dueDate, dto.ActivityDate);
        Assert.AreEqual("Monthly Condo Fee", dto.Description);
        Assert.AreEqual(100m, dto.Charge);
        Assert.IsNull(dto.Payment);
        Assert.AreEqual(0m, dto.Balance);
    }

    [TestMethod]
    public void RegisterMaps_UnitMapFunctionWithNoOwnerAndNoActivities_ReturnsNAOwnerInfoAndPaidInFull()
    {
        // Arrange
        var recordingMapper = new RecordingMapper();
        var sut = new StatementMapper();
        sut.RegisterMaps(recordingMapper);

        var unit = CreateUnit("303", "789 Pine Rd");

        // Act
        var dto = recordingMapper.Map<CondoScope.Application.Statement.StatementDto>(unit);

        // Assert
        Assert.AreEqual("303", dto.UnitNumber);
        Assert.AreEqual("789 Pine Rd", dto.Address);
        Assert.AreEqual("N/A", dto.OwnerName);
        Assert.AreEqual("N/A", dto.EmailAddress);
        Assert.AreEqual("N/A", dto.PhoneNumber);
        Assert.AreEqual(0m, dto.CurrentBalance);
        Assert.AreEqual(AccountStatus.PaidInFull, dto.AccountStatus);
        Assert.AreEqual(0, dto.AccountActivities.Count());
    }

    [TestMethod]
    public void RegisterMaps_UnitMapFunctionWithOwnerHavingEmailAndPhone_MapsOwnerContactInfo()
    {
        // Arrange
        var recordingMapper = new RecordingMapper();
        var sut = new StatementMapper();
        sut.RegisterMaps(recordingMapper);

        var unit = CreateUnit("404", "111 Elm St");
        var email = Email.Create("owner@example.com").Value;
        var phone = PhoneNumber.Create("+15145551234").Value;
        var owner = Owner.Create("John Smith", email, phone, "tester").Value;
        unit.AssignOwner(owner, DateOnly.FromDateTime(DateTime.UtcNow), "tester");

        // Act
        var dto = recordingMapper.Map<CondoScope.Application.Statement.StatementDto>(unit);

        // Assert
        Assert.AreEqual("John Smith", dto.OwnerName);
        Assert.AreEqual("owner@example.com", dto.EmailAddress);
        Assert.AreEqual(phone.Value, dto.PhoneNumber);
    }

    [TestMethod]
    public void RegisterMaps_UnitMapFunctionWithOwnerHavingNoEmailOrPhone_ReturnsNAForContactInfo()
    {
        // Arrange
        var recordingMapper = new RecordingMapper();
        var sut = new StatementMapper();
        sut.RegisterMaps(recordingMapper);

        var unit = CreateUnit("505", "222 Birch Ln");
        var owner = Owner.Create("Jane Doe", null, null, "tester").Value;
        unit.AssignOwner(owner, DateOnly.FromDateTime(DateTime.UtcNow), "tester");

        // Act
        var dto = recordingMapper.Map<CondoScope.Application.Statement.StatementDto>(unit);

        // Assert
        Assert.AreEqual("Jane Doe", dto.OwnerName);
        Assert.AreEqual("N/A", dto.EmailAddress);
        Assert.AreEqual("N/A", dto.PhoneNumber);
    }

    private static Unit CreateUnit(string unitNumber, string address)
    {
        var result = Unit.Create(unitNumber, address, true, "tester");
        return result.Value;
    }

    private sealed class RecordingMapper : IMapper
    {
        public List<(Type Source, Type Destination)> Registrations { get; } = [];

        private readonly Dictionary<(Type Source, Type Destination), Delegate> mappingFunctions = [];

        public TDestination Map<TDestination>(object source)
        {
            var key = (source.GetType(), typeof(TDestination));
            if (!mappingFunctions.TryGetValue(key, out var mapFunction))
            {
                throw new InvalidOperationException($"No mapping function registered for {key.Item1} to {key.Item2}.");
            }

            return (TDestination)mapFunction.DynamicInvoke(source)!;
        }

        public IEnumerable<TDestination> Map<TDestination>(IEnumerable source)
        {
            var list = new List<TDestination>();
            foreach (var item in source)
            {
                if (item is null) continue;
                list.Add(this.Map<TDestination>(item));
            }

            return list;
        }

        public IMapper Register<TSource, TDestination>(Func<TSource, TDestination> mapFunction)
        {
            this.Registrations.Add((typeof(TSource), typeof(TDestination)));
            this.mappingFunctions[(typeof(TSource), typeof(TDestination))] = mapFunction;
            return this;
        }
    }
}
