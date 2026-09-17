using System.Collections;
using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Ledger;
using CondoScope.Domain.Entities;

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
        Assert.AreEqual(1, fakeMapper.RegisteredSourceTypes.Count);
        Assert.AreEqual(typeof(Unit), fakeMapper.RegisteredSourceTypes[0]);
        Assert.AreEqual(typeof(LedgerDTO), fakeMapper.RegisteredDestinationTypes[0]);
    }

    private sealed class FakeMapper : IMapper
    {
        public List<Type> RegisteredSourceTypes { get; } = [];

        public List<Type> RegisteredDestinationTypes { get; } = [];

        public TDestination Map<TDestination>(object source) => throw new NotSupportedException();

        public IEnumerable<TDestination> Map<TDestination>(IEnumerable source) => throw new NotSupportedException();

        public IMapper Register<TSource, TDestination>(Func<TSource, TDestination> mapFunction)
        {
            this.RegisteredSourceTypes.Add(typeof(TSource));
            this.RegisteredDestinationTypes.Add(typeof(TDestination));
            return this;
        }
    }
}
