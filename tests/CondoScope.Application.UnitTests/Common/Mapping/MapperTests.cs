using CondoScope.Application.Common.Mapping;

namespace CondoScope.Application.UnitTests.Common.Mapping;

[TestClass]
public class MapperTests
{
    private sealed class Source
    {
        public int Value { get; set; }
    }

    private sealed class Destination
    {
        public int Value { get; set; }
    }

    [TestMethod]
    public void Map_Single_SourceIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mapper = new Mapper();
        object? source = null;

        // Act
        var act = () => mapper.Map<Destination>(source!);

        // Assert
        Assert.ThrowsExactly<ArgumentNullException>(act);
    }

    [TestMethod]
    public void Map_Single_NoMappingRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var mapper = new Mapper();
        var source = new Source { Value = 1 };

        // Act
        var act = () => mapper.Map<Destination>(source);

        // Assert
        var ex = Assert.ThrowsExactly<InvalidOperationException>(act);
        StringAssert.Contains(ex.Message, "No mapping function registered");
        StringAssert.Contains(ex.Message, nameof(Source));
        StringAssert.Contains(ex.Message, nameof(Destination));
    }

    [TestMethod]
    public void Map_Single_MappingRegistered_ReturnsMappedDestination()
    {
        // Arrange
        var mapper = new Mapper();
        mapper.Register<Source, Destination>(s => new Destination { Value = s.Value * 2 });
        var source = new Source { Value = 5 };

        // Act
        var result = mapper.Map<Destination>(source);

        // Assert
        Assert.AreEqual(10, result.Value);
    }

    [TestMethod]
    public void Map_Enumerable_SourceIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mapper = new Mapper();
        System.Collections.IEnumerable? source = null;

        // Act
        var act = () => mapper.Map<Destination>(source!);

        // Assert
        Assert.ThrowsExactly<ArgumentNullException>(act);
    }

    [TestMethod]
    public void Map_Enumerable_WithItems_ReturnsMappedList()
    {
        // Arrange
        var mapper = new Mapper();
        mapper.Register<Source, Destination>(s => new Destination { Value = s.Value + 1 });
        var sources = new List<Source>
        {
            new() { Value = 1 },
            new() { Value = 2 },
            new() { Value = 3 },
        };

        // Act
        var result = mapper.Map<Destination>(sources).ToList();

        // Assert
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEqual(new[] { 2, 3, 4 }, result.Select(r => r.Value).ToList());
    }

    [TestMethod]
    public void Map_Enumerable_SkipsNullItems()
    {
        // Arrange
        var mapper = new Mapper();
        mapper.Register<Source, Destination>(s => new Destination { Value = s.Value });
        var sources = new List<Source?> { new() { Value = 1 }, null, new() { Value = 2 } };

        // Act
        var result = mapper.Map<Destination>(sources).ToList();

        // Assert
        Assert.AreEqual(2, result.Count);
        CollectionAssert.AreEqual(new[] { 1, 2 }, result.Select(r => r.Value).ToList());
    }

    [TestMethod]
    public void Map_Enumerable_EmptySource_ReturnsEmptyList()
    {
        // Arrange
        var mapper = new Mapper();
        var sources = new List<Source>();

        // Act
        var result = mapper.Map<Destination>(sources).ToList();

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Register_MapFunctionIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var mapper = new Mapper();
        Func<Source, Destination>? mapFunction = null;

        // Act
        var act = () => mapper.Register(mapFunction!);

        // Assert
        Assert.ThrowsExactly<ArgumentNullException>(act);
    }

    [TestMethod]
    public void Register_ValidMapFunction_ReturnsSameMapperInstance()
    {
        // Arrange
        var mapper = new Mapper();

        // Act
        var result = mapper.Register<Source, Destination>(s => new Destination { Value = s.Value });

        // Assert
        Assert.AreSame(mapper, result);
    }

    [TestMethod]
    public void Register_CalledTwiceForSameTypePair_OverwritesPreviousMapping()
    {
        // Arrange
        var mapper = new Mapper();
        mapper.Register<Source, Destination>(s => new Destination { Value = 100 });
        mapper.Register<Source, Destination>(s => new Destination { Value = 200 });
        var source = new Source { Value = 1 };

        // Act
        var result = mapper.Map<Destination>(source);

        // Assert
        Assert.AreEqual(200, result.Value);
    }

    [TestMethod]
    public void Build_ConfigureIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        Action<Mapper>? configure = null;

        // Act
        var act = () => Mapper.Build(configure!);

        // Assert
        Assert.ThrowsExactly<ArgumentNullException>(act);
    }

    [TestMethod]
    public void Build_ValidConfigure_InvokesConfigureAndReturnsConfiguredMapper()
    {
        // Arrange
        var invoked = false;

        // Act
        var mapper = Mapper.Build(m =>
        {
            invoked = true;
            m.Register<Source, Destination>(s => new Destination { Value = s.Value });
        });

        // Assert
        Assert.IsTrue(invoked);
        Assert.IsNotNull(mapper);
        var result = mapper.Map<Destination>(new Source { Value = 7 });
        Assert.AreEqual(7, result.Value);
    }
}
