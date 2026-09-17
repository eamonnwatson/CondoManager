using System.ComponentModel.DataAnnotations;
using System.Linq;
using CondoScope.Web.Components.Dialogs.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CondoScope.Web.UnitTests.Components.Dialogs.Models;

[TestClass]
public class AddOwnerFormModelTests
{
    [TestMethod]
    public void Validate_UnitIdSetAndEffectiveDateNull_ReturnsValidationError()
    {
        // Arrange
        var model = new AddOwnerFormModel
        {
            Name = "John Doe",
            UnitId = "Unit-1",
            EffectiveDate = null,
        };
        var context = new ValidationContext(model);

        // Act
        var results = model.Validate(context).ToList();

        // Assert
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Effective Date is required when a Unit is selected.", results[0].ErrorMessage);
        CollectionAssert.AreEqual(new[] { nameof(AddOwnerFormModel.EffectiveDate) }, results[0].MemberNames.ToArray());
    }

    [TestMethod]
    public void Validate_UnitIdWhitespaceAndEffectiveDateNull_ReturnsValidationError()
    {
        // Arrange
        var model = new AddOwnerFormModel
        {
            Name = "John Doe",
            UnitId = "   ",
            EffectiveDate = null,
        };
        var context = new ValidationContext(model);

        // Act
        var results = model.Validate(context).ToList();

        // Assert - whitespace-only UnitId is treated as not selected, so no error expected
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void Validate_UnitIdNullAndEffectiveDateNull_ReturnsNoValidationError()
    {
        // Arrange
        var model = new AddOwnerFormModel
        {
            Name = "John Doe",
            UnitId = null,
            EffectiveDate = null,
        };
        var context = new ValidationContext(model);

        // Act
        var results = model.Validate(context).ToList();

        // Assert
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void Validate_UnitIdSetAndEffectiveDateSet_ReturnsNoValidationError()
    {
        // Arrange
        var model = new AddOwnerFormModel
        {
            Name = "John Doe",
            UnitId = "Unit-1",
            EffectiveDate = DateTime.UtcNow,
        };
        var context = new ValidationContext(model);

        // Act
        var results = model.Validate(context).ToList();

        // Assert
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void Validate_UnitIdEmptyAndEffectiveDateNull_ReturnsNoValidationError()
    {
        // Arrange
        var model = new AddOwnerFormModel
        {
            Name = "John Doe",
            UnitId = string.Empty,
            EffectiveDate = null,
        };
        var context = new ValidationContext(model);

        // Act
        var results = model.Validate(context).ToList();

        // Assert
        Assert.AreEqual(0, results.Count);
    }
}
