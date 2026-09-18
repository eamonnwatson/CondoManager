using System.Collections;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Ledger;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;

namespace CondoScope.Application.UnitTests.Ledger;

[TestClass]
public class LedgerMapperTests
{
    [TestMethod]
    public void RegisterMaps_WhenCalled_RegistersUnitToLedgerDtoMap()
    {
        // Arrange
        var fakeMapper = new FakeMapper();
        var sut = new LedgerMapper();

        // Act
        sut.RegisterMaps(fakeMapper);

        // Assert
        Assert.AreEqual(2, fakeMapper.RegisteredSourceTypes.Count);
        Assert.AreEqual(typeof(Unit), fakeMapper.RegisteredSourceTypes[0]);
        Assert.AreEqual(typeof(LedgerDTO), fakeMapper.RegisteredDestinationTypes[0]);
    }

    [TestMethod]
    [DataRow(AccountStatus.Credit, "Credit")]
    [DataRow(AccountStatus.PaidInFull, "Paid")]
    [DataRow(AccountStatus.Outstanding, "Outstanding")]
    public void RegisterMaps_WhenAccountStatusMapInvoked_ReturnsExpectedString(AccountStatus status, string expected)
    {
        // Arrange
        var fakeMapper = new FakeMapper();
        var sut = new LedgerMapper();
        sut.RegisterMaps(fakeMapper);
        var accountStatusMapper = (Func<AccountStatus, string>)fakeMapper.RegisteredFunctions[1];

        // Act
        var result = accountStatusMapper(status);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void RegisterMaps_WhenAccountStatusMapInvokedWithInvalidValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var fakeMapper = new FakeMapper();
        var sut = new LedgerMapper();
        sut.RegisterMaps(fakeMapper);
        var accountStatusMapper = (Func<AccountStatus, string>)fakeMapper.RegisteredFunctions[1];
        var invalidStatus = (AccountStatus)999;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => accountStatusMapper(invalidStatus));
    }

    private sealed class FakeMapper : IMapper
    {
        public List<Type> RegisteredSourceTypes { get; } = [];

        public List<Type> RegisteredDestinationTypes { get; } = [];

        public List<Delegate> RegisteredFunctions { get; } = [];

        public TDestination Map<TDestination>(object source) => throw new NotSupportedException();

        public IEnumerable<TDestination> Map<TDestination>(IEnumerable source) => throw new NotSupportedException();

        public IMapper Register<TSource, TDestination>(Func<TSource, TDestination> mapFunction)
        {
            this.RegisteredSourceTypes.Add(typeof(TSource));
            this.RegisteredDestinationTypes.Add(typeof(TDestination));
            this.RegisteredFunctions.Add(mapFunction);
            return this;
        }
    }
}
