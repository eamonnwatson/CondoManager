using CondoScope.Application.Common.Errors;
using FluentResults;

namespace CondoScope.Application.UnitTests.Common.Errors;

[TestClass]
public class ResultErrorExtensionsTests
{
    [TestMethod]
    public void WithContext_ResultHasSingleError_AddsMetadataToError()
    {
        // Arrange
        var error = new Error("Something went wrong");
        var result = Result.Fail(error);
        var ctx = new ErrorContext("trace-1", "FeatureA", "HandlerA", "LayerA");

        // Act
        var returned = result.WithContext(ctx);

        // Assert
        Assert.AreSame(result, returned);
        var actualError = returned.Errors[0];
        Assert.AreEqual("trace-1", actualError.Metadata["TraceId"]);
        Assert.AreEqual("FeatureA", actualError.Metadata["Feature"]);
        Assert.AreEqual("HandlerA", actualError.Metadata["Handler"]);
        Assert.AreEqual("LayerA", actualError.Metadata["Layer"]);
    }

    [TestMethod]
    public void WithContext_ResultHasMultipleErrors_AddsMetadataToAllErrors()
    {
        // Arrange
        var error1 = new Error("Error one");
        var error2 = new Error("Error two");
        var result = Result.Fail(new List<IError> { error1, error2 });
        var ctx = new ErrorContext("trace-2", "FeatureB", "HandlerB", "LayerB");

        // Act
        var returned = result.WithContext(ctx);

        // Assert
        Assert.AreEqual(2, returned.Errors.Count);
        foreach (var err in returned.Errors)
        {
            Assert.AreEqual("trace-2", err.Metadata["TraceId"]);
            Assert.AreEqual("FeatureB", err.Metadata["Feature"]);
            Assert.AreEqual("HandlerB", err.Metadata["Handler"]);
            Assert.AreEqual("LayerB", err.Metadata["Layer"]);
        }
    }

    [TestMethod]
    public void WithContext_ResultHasNoErrors_ReturnsResultUnchanged()
    {
        // Arrange
        var result = Result.Ok();
        var ctx = new ErrorContext("trace-3", "FeatureC", "HandlerC", "LayerC");

        // Act
        var returned = result.WithContext(ctx);

        // Assert
        Assert.AreSame(result, returned);
        Assert.AreEqual(0, returned.Errors.Count);
    }

    [TestMethod]
    public void WithContext_ErrorAlreadyHasMetadata_OverwritesExistingKeys()
    {
        // Arrange
        var error = new Error("Existing metadata error")
            .WithMetadata("TraceId", "old-trace")
            .WithMetadata("CustomKey", "customValue");
        var result = Result.Fail(error);
        var ctx = new ErrorContext("new-trace", "FeatureD", "HandlerD", "LayerD");

        // Act
        var returned = result.WithContext(ctx);

        // Assert
        var actualError = returned.Errors[0];
        Assert.AreEqual("new-trace", actualError.Metadata["TraceId"]);
        Assert.AreEqual("FeatureD", actualError.Metadata["Feature"]);
        Assert.AreEqual("HandlerD", actualError.Metadata["Handler"]);
        Assert.AreEqual("LayerD", actualError.Metadata["Layer"]);
        Assert.AreEqual("customValue", actualError.Metadata["CustomKey"]);
    }
}
