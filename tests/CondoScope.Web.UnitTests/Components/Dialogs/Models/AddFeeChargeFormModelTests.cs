using System.ComponentModel.DataAnnotations;
using CondoScope.Web.Components.Dialogs.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CondoScope.Web.UnitTests.Components.Dialogs.Models;

[TestClass]
public class AddFeeChargeFormModelTests
{
    [TestMethod]
    public void Validate_ScopeIsSpecificUnitAndUnitIdNull_ReturnsValidationError()
    {
        var model = new AddFeeChargeFormModel { Scope = 1, UnitId = null };
        var context = new ValidationContext(model);

        var results = model.Validate(context).ToList();

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Unit is required when Scope is set to Specific Unit.", results[0].ErrorMessage);
        CollectionAssert.AreEqual(new[] { nameof(AddFeeChargeFormModel.UnitId) }, results[0].MemberNames.ToList());
    }

    [TestMethod]
    public void Validate_ScopeIsSpecificUnitAndUnitIdEmpty_ReturnsValidationError()
    {
        var model = new AddFeeChargeFormModel { Scope = 1, UnitId = string.Empty };
        var context = new ValidationContext(model);

        var results = model.Validate(context).ToList();

        Assert.AreEqual(1, results.Count);
    }

    [TestMethod]
    public void Validate_ScopeIsSpecificUnitAndUnitIdWhitespace_ReturnsValidationError()
    {
        var model = new AddFeeChargeFormModel { Scope = 1, UnitId = "   " };
        var context = new ValidationContext(model);

        var results = model.Validate(context).ToList();

        Assert.AreEqual(1, results.Count);
    }

    [TestMethod]
    public void Validate_ScopeIsSpecificUnitAndUnitIdProvided_ReturnsNoErrors()
    {
        var model = new AddFeeChargeFormModel { Scope = 1, UnitId = "unit-1" };
        var context = new ValidationContext(model);

        var results = model.Validate(context).ToList();

        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void Validate_ScopeIsNotSpecificUnitAndUnitIdNull_ReturnsNoErrors()
    {
        var model = new AddFeeChargeFormModel { Scope = 0, UnitId = null };
        var context = new ValidationContext(model);

        var results = model.Validate(context).ToList();

        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void Validate_ScopeIsNotSpecificUnitAndUnitIdProvided_ReturnsNoErrors()
    {
        var model = new AddFeeChargeFormModel { Scope = 2, UnitId = "unit-1" };
        var context = new ValidationContext(model);

        var results = model.Validate(context).ToList();

        Assert.AreEqual(0, results.Count);
    }
}
