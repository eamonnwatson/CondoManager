using CondoScope.Application.Common.Mapping;
using CondoScope.Application.Statement.Pdf;
using Microsoft.Extensions.DependencyInjection;

namespace CondoScope.Application.UnitTests;

[TestClass]
public class DependencyInjectionTests
{
    [TestMethod]
    public void AddApplication_Called_ReturnsSameServiceCollectionInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplication();

        // Assert
        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddApplication_Called_RegistersIMapperAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IMapper));
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [TestMethod]
    public void AddApplication_Called_RegistersIStatementPdfGeneratorAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStatementPdfGenerator));
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.AreEqual(typeof(StatementPdfGenerator), descriptor.ImplementationType);
    }

    [TestMethod]
    public void AddApplication_BuiltProvider_ResolvesIMapperInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddApplication();
        var provider = services.BuildServiceProvider();

        // Act
        var mapper = provider.GetService<IMapper>();

        // Assert
        Assert.IsNotNull(mapper);
        Assert.IsInstanceOfType<Mapper>(mapper);
    }

    [TestMethod]
    public void AddApplication_BuiltProvider_ResolvesIStatementPdfGeneratorInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddApplication();
        var provider = services.BuildServiceProvider();

        // Act
        var generator = provider.GetService<IStatementPdfGenerator>();

        // Assert
        Assert.IsNotNull(generator);
        Assert.IsInstanceOfType<StatementPdfGenerator>(generator);
    }
}
