using CondoScope.Application.Common.Mapping;
using CondoScope.Application.FeeCharges;
using CondoScope.Application.Units;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using Moq;

namespace CondoScope.Application.UnitTests.FeeCharges;

[TestClass]
public class FeeChargeMapperTests
{
    private readonly Mock<IMapper> mapperMock = new(MockBehavior.Strict);

    private static Unit CreateUnit(string unitNumber = "101") =>
        Unit.Create(unitNumber, "123 Main St", true, "creator").Value;

    private static FeeCharge CreateFeeCharge(Unit unit) =>
        FeeCharge.Create(100m, new DateOnly(2024, 3, 1), "March Fee", ChargeCategory.CondoFee,
            ChargeScope.SpecificUnit, unit, "creator").Value;

    [TestMethod]
    public void RegisterMaps_WhenCalled_RegistersFeeChargeToFeeChargeDtoMap()
    {
        // Arrange
        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<FeeCharge, FeeChargeDto>>()))
            .Returns(mapperMock.Object);

        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<ChargeCategory, string>>()))
            .Returns(mapperMock.Object);

        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<ChargeScope, string>>()))
            .Returns(mapperMock.Object);

        var mapper = new FeeChargeMapper();

        // Act
        mapper.RegisterMaps(mapperMock.Object);

        // Assert
        mapperMock.Verify(m => m.Register(It.IsAny<Func<FeeCharge, FeeChargeDto>>()), Times.Once);
    }

    [TestMethod]
    public void RegisterMaps_WhenRegisteredFunctionInvoked_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var unit = CreateUnit();
        var feeCharge = CreateFeeCharge(unit);
        var expectedUnitDtos = new[]
        {
            new UnitDto(unit.Id.ToString(), unit.UnitNumber, unit.Address, string.Empty, string.Empty),
        };

        Func<FeeCharge, FeeChargeDto>? capturedMapFunction = null;
        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<FeeCharge, FeeChargeDto>>()))
            .Callback<Func<FeeCharge, FeeChargeDto>>(f => capturedMapFunction = f)
            .Returns(mapperMock.Object);

        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<ChargeCategory, string>>()))
            .Returns(mapperMock.Object);

        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<ChargeScope, string>>()))
            .Returns(mapperMock.Object);

        mapperMock
            .Setup(m => m.Map<UnitDto>(feeCharge.Units))
            .Returns(expectedUnitDtos);

        var mapper = new FeeChargeMapper();
        mapper.RegisterMaps(mapperMock.Object);

        Assert.IsNotNull(capturedMapFunction);

        // Act
        var dto = capturedMapFunction!(feeCharge);

        // Assert
        Assert.AreEqual(feeCharge.Id.ToString(), dto.Id);
        Assert.AreEqual(feeCharge.DueDate, dto.DueDate);
        Assert.AreEqual(feeCharge.Description, dto.Description);
        Assert.AreEqual(feeCharge.Category, dto.Category);
        Assert.AreEqual(feeCharge.Scope, dto.Scope);
        Assert.AreEqual(feeCharge.Amount, dto.Amount);
        CollectionAssert.AreEqual(expectedUnitDtos, dto.Units.ToList());

        mapperMock.Verify(m => m.Map<UnitDto>(feeCharge.Units), Times.Once);
    }

    [TestMethod]
    [DataRow(ChargeCategory.CondoFee, "Condo Fee")]
    [DataRow(ChargeCategory.ReserveFee, "Reserve Fund")]
    [DataRow(ChargeCategory.OtherFee, "Other Fee")]
    public void RegisterMaps_WhenCategoryFunctionInvoked_MapsCategoryToExpectedString(
        ChargeCategory category, string expected)
    {
        // Arrange
        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<FeeCharge, FeeChargeDto>>()))
            .Returns(mapperMock.Object);

        Func<ChargeCategory, string>? capturedFunction = null;
        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<ChargeCategory, string>>()))
            .Callback<Func<ChargeCategory, string>>(f => capturedFunction = f)
            .Returns(mapperMock.Object);

        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<ChargeScope, string>>()))
            .Returns(mapperMock.Object);

        var mapper = new FeeChargeMapper();
        mapper.RegisterMaps(mapperMock.Object);

        Assert.IsNotNull(capturedFunction);

        // Act
        var result = capturedFunction!(category);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void RegisterMaps_WhenCategoryFunctionInvokedWithUnknownValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<FeeCharge, FeeChargeDto>>()))
            .Returns(mapperMock.Object);

        Func<ChargeCategory, string>? capturedFunction = null;
        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<ChargeCategory, string>>()))
            .Callback<Func<ChargeCategory, string>>(f => capturedFunction = f)
            .Returns(mapperMock.Object);

        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<ChargeScope, string>>()))
            .Returns(mapperMock.Object);

        var mapper = new FeeChargeMapper();
        mapper.RegisterMaps(mapperMock.Object);

        Assert.IsNotNull(capturedFunction);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => capturedFunction!((ChargeCategory)999));
    }

    [TestMethod]
    [DataRow(ChargeScope.AllUnits, "All Units")]
    [DataRow(ChargeScope.SpecificUnit, "Specific Unit")]
    public void RegisterMaps_WhenScopeFunctionInvoked_MapsScopeToExpectedString(
        ChargeScope scope, string expected)
    {
        // Arrange
        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<FeeCharge, FeeChargeDto>>()))
            .Returns(mapperMock.Object);

        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<ChargeCategory, string>>()))
            .Returns(mapperMock.Object);

        Func<ChargeScope, string>? capturedFunction = null;
        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<ChargeScope, string>>()))
            .Callback<Func<ChargeScope, string>>(f => capturedFunction = f)
            .Returns(mapperMock.Object);

        var mapper = new FeeChargeMapper();
        mapper.RegisterMaps(mapperMock.Object);

        Assert.IsNotNull(capturedFunction);

        // Act
        var result = capturedFunction!(scope);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void RegisterMaps_WhenScopeFunctionInvokedWithUnknownValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<FeeCharge, FeeChargeDto>>()))
            .Returns(mapperMock.Object);

        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<ChargeCategory, string>>()))
            .Returns(mapperMock.Object);

        Func<ChargeScope, string>? capturedFunction = null;
        mapperMock
            .Setup(m => m.Register(It.IsAny<Func<ChargeScope, string>>()))
            .Callback<Func<ChargeScope, string>>(f => capturedFunction = f)
            .Returns(mapperMock.Object);

        var mapper = new FeeChargeMapper();
        mapper.RegisterMaps(mapperMock.Object);

        Assert.IsNotNull(capturedFunction);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => capturedFunction!((ChargeScope)999));
    }
}
