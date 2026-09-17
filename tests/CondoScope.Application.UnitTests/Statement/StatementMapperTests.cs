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

    [TestMethod]
    public void RegisterMaps_UnitMapFunctionWithActivityInFuture_FiltersOutFutureActivityAndExcludesFromBalance()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var pastActivity = new AccountActivityDto(today.AddDays(-5), "Past Fee", 50m, null, 0);
        var futureActivity = new AccountActivityDto(today.AddDays(5), "Future Fee", 999m, null, 0);

        var sequencedMapper = new SequencedActivityMapper([pastActivity], [futureActivity]);
        var sut = new StatementMapper();
        sut.RegisterMaps(sequencedMapper);
        var unit = CreateUnit("601", "1 Filter Ln");

        // Act
        var dto = sequencedMapper.Map<CondoScope.Application.Statement.StatementDto>(unit);

        // Assert
        var activities = dto.AccountActivities.ToList();
        Assert.AreEqual(1, activities.Count);
        Assert.AreEqual("Past Fee", activities[0].Description);
        Assert.AreEqual(50m, dto.CurrentBalance);
        Assert.AreEqual(AccountStatus.Outstanding, dto.AccountStatus);
    }

    [TestMethod]
    public void RegisterMaps_UnitMapFunctionWithChargeAndPaymentOnSameDate_OrdersChargeBeforePaymentAndAccumulatesBalance()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var sameDate = today.AddDays(-1);
        var charge = new AccountActivityDto(sameDate, "Monthly Fee", 100m, null, 0);
        var payment = new AccountActivityDto(sameDate, "Payment Received", null, 40m, 0);

        var sequencedMapper = new SequencedActivityMapper([payment], [charge]);
        var sut = new StatementMapper();
        sut.RegisterMaps(sequencedMapper);
        var unit = CreateUnit("602", "2 Order Ln");

        // Act
        var dto = sequencedMapper.Map<CondoScope.Application.Statement.StatementDto>(unit);

        // Assert
        var activities = dto.AccountActivities.ToList();
        Assert.AreEqual(2, activities.Count);
        Assert.AreEqual("Monthly Fee", activities[0].Description);
        Assert.AreEqual(100m, activities[0].Balance);
        Assert.AreEqual("Payment Received", activities[1].Description);
        Assert.AreEqual(60m, activities[1].Balance);
        Assert.AreEqual(60m, dto.CurrentBalance);
        Assert.AreEqual(AccountStatus.Outstanding, dto.AccountStatus);
    }

    [TestMethod]
    public void RegisterMaps_UnitMapFunctionWithActivitiesAcrossMultipleDates_OrdersByDateAndAccumulatesRunningBalance()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var earlierCharge = new AccountActivityDto(today.AddDays(-10), "First Fee", 100m, null, 0);
        var laterPayment = new AccountActivityDto(today.AddDays(-2), "First Payment", null, 100m, 0);

        var sequencedMapper = new SequencedActivityMapper([laterPayment], [earlierCharge]);
        var sut = new StatementMapper();
        sut.RegisterMaps(sequencedMapper);
        var unit = CreateUnit("603", "3 Date Ln");

        // Act
        var dto = sequencedMapper.Map<CondoScope.Application.Statement.StatementDto>(unit);

        // Assert
        var activities = dto.AccountActivities.ToList();
        Assert.AreEqual(2, activities.Count);
        Assert.AreEqual("First Fee", activities[0].Description);
        Assert.AreEqual(100m, activities[0].Balance);
        Assert.AreEqual("First Payment", activities[1].Description);
        Assert.AreEqual(0m, activities[1].Balance);
        Assert.AreEqual(0m, dto.CurrentBalance);
        Assert.AreEqual(AccountStatus.PaidInFull, dto.AccountStatus);
    }

    [TestMethod]
    public void RegisterMaps_UnitMapFunctionWithMorePaymentsThanCharges_ResultsInCreditStatus()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var charge = new AccountActivityDto(today.AddDays(-5), "Fee", 50m, null, 0);
        var payment = new AccountActivityDto(today.AddDays(-3), "Payment", null, 200m, 0);

        var sequencedMapper = new SequencedActivityMapper([payment], [charge]);
        var sut = new StatementMapper();
        sut.RegisterMaps(sequencedMapper);
        var unit = CreateUnit("604", "4 Credit Ln");

        // Act
        var dto = sequencedMapper.Map<CondoScope.Application.Statement.StatementDto>(unit);

        // Assert
        Assert.AreEqual(-150m, dto.CurrentBalance);
        Assert.AreEqual(AccountStatus.Credit, dto.AccountStatus);
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

    /// <summary>
    /// A fake mapper used to exercise the Unit map function's internal balance/ordering logic.
    /// Since Unit.Payments and Unit.FeeCharges cannot be populated without EF change-tracking,
    /// this mapper intercepts the two sequential Map&lt;TDestination&gt;(IEnumerable) calls made
    /// inside the Unit map function (first for Payments, then for FeeCharges) and substitutes
    /// pre-built AccountActivityDto collections, regardless of the actual (always-empty) source content.
    /// </summary>
    private sealed class SequencedActivityMapper : IMapper
    {
        private readonly Dictionary<(Type Source, Type Destination), Delegate> mappingFunctions = [];
        private readonly Queue<IEnumerable<AccountActivityDto>> enumerableResults = new();

        public SequencedActivityMapper(IEnumerable<AccountActivityDto> paymentsResult, IEnumerable<AccountActivityDto> feeChargesResult)
        {
            enumerableResults.Enqueue(paymentsResult);
            enumerableResults.Enqueue(feeChargesResult);
        }

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
            var result = enumerableResults.Dequeue();
            return (IEnumerable<TDestination>)(object)result;
        }

        public IMapper Register<TSource, TDestination>(Func<TSource, TDestination> mapFunction)
        {
            this.mappingFunctions[(typeof(TSource), typeof(TDestination))] = mapFunction;
            return this;
        }
    }
}
