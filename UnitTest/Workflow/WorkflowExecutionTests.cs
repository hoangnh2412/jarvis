using Platform.Workflow.Abstractions;

namespace UnitTest.Workflow;

/// <summary>
/// Unit tests cho IWorkflowService - Workflow execution pipeline
/// </summary>
public class WorkflowExecutionTests
{
    #region WorkflowExecutionResult Tests

    [Fact]
    public void WorkflowExecutionResult_Creation_WithAllParameters()
    {
        // Arrange
        const string instanceId = "instance-123";
        const string status = "Completed";
        var output = new Dictionary<string, object> { { "key", "value" } };

        // Act
        var result = new WorkflowExecutionResult(instanceId, status, output, false, null);

        // Assert
        Assert.Equal(instanceId, result.WorkflowInstanceId);
        Assert.Equal(status, result.Status);
        Assert.NotNull(result.Output);
        Assert.False(result.IsFaulted);
        Assert.Null(result.FaultMessage);
    }

    [Fact]
    public void WorkflowExecutionResult_WithFault_ShouldHaveFaultInfo()
    {
        // Arrange
        const string faultMessage = "Workflow execution failed";

        // Act
        var result = new WorkflowExecutionResult(null, "Faulted", null, true, faultMessage);

        // Assert
        Assert.Null(result.WorkflowInstanceId);
        Assert.Equal("Faulted", result.Status);
        Assert.True(result.IsFaulted);
        Assert.Equal(faultMessage, result.FaultMessage);
    }

    [Fact]
    public void WorkflowExecutionResult_WithNullOutput_ShouldBeValid()
    {
        // Arrange & Act
        var result = new WorkflowExecutionResult("instance-1", "Completed", null);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Output);
    }

    [Fact]
    public void WorkflowExecutionResult_WithComplexOutput_ShouldPreserveData()
    {
        // Arrange
        var complexOutput = new Dictionary<string, object>
        {
            { "id", 123 },
            { "name", "Test" },
            { "items", new List<string> { "a", "b", "c" } }
        };

        // Act
        var result = new WorkflowExecutionResult("inst-1", "Completed", complexOutput);

        // Assert
        Assert.NotNull(result.Output);
        Assert.Equal(3, result.Output.Count);
        Assert.Equal(123, result.Output["id"]);
    }

    #endregion

    #region Workflow Status Tests

    [Theory]
    [InlineData("Completed")]
    [InlineData("Faulted")]
    [InlineData("Suspended")]
    [InlineData("Running")]
    public void WorkflowExecutionResult_WithDifferentStatuses_ShouldBeValid(string status)
    {
        // Arrange & Act
        var result = new WorkflowExecutionResult("inst-1", status);

        // Assert
        Assert.Equal(status, result.Status);
    }

    #endregion

    #region Multiple Results Tests

    [Fact]
    public void WorkflowExecutionResult_MultipleInstances_ShouldBeIndependent()
    {
        // Arrange & Act
        var result1 = new WorkflowExecutionResult("inst-1", "Completed");
        var result2 = new WorkflowExecutionResult("inst-2", "Faulted");

        // Assert
        Assert.NotEqual(result1.WorkflowInstanceId, result2.WorkflowInstanceId);
        Assert.NotEqual(result1.Status, result2.Status);
    }

    [Fact]
    public void WorkflowExecutionResult_WithDifferentOutputs_ShouldMaintainSeparation()
    {
        // Arrange
        var output1 = new Dictionary<string, object> { { "result", "A" } };
        var output2 = new Dictionary<string, object> { { "result", "B" } };

        // Act
        var result1 = new WorkflowExecutionResult("inst-1", "Completed", output1);
        var result2 = new WorkflowExecutionResult("inst-2", "Completed", output2);

        // Assert
        Assert.NotEqual(result1.Output?["result"], result2.Output?["result"]);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void WorkflowExecutionResult_WithEmptyInstanceId_ShouldBeValid()
    {
        // Arrange & Act
        var result = new WorkflowExecutionResult(null, "Pending");

        // Assert
        Assert.Null(result.WorkflowInstanceId);
    }

    [Fact]
    public void WorkflowExecutionResult_WithEmptyOutput_ShouldBeValid()
    {
        // Arrange & Act
        var result = new WorkflowExecutionResult("inst-1", "Completed", new Dictionary<string, object>());

        // Assert
        Assert.NotNull(result.Output);
        Assert.Empty(result.Output);
    }

    [Fact]
    public void WorkflowExecutionResult_WithSpecialCharactersInOutput_ShouldBePreserved()
    {
        // Arrange
        var output = new Dictionary<string, object>
        {
            { "special", "!@#$%^&*()" },
            { "unicode", "ñáéíóú" }
        };

        // Act
        var result = new WorkflowExecutionResult("inst-1", "Completed", output);

        // Assert
        Assert.Equal("!@#$%^&*()", result.Output?["special"]);
        Assert.Equal("ñáéíóú", result.Output?["unicode"]);
    }

    #endregion

    #region Stress Tests

    [Fact]
    public void WorkflowExecutionResult_WithLargeOutput_ShouldBeValid()
    {
        // Arrange
        var largeOutput = new Dictionary<string, object>();
        for (int i = 0; i < 1000; i++)
        {
            largeOutput[$"key_{i}"] = $"value_{i}";
        }

        // Act
        var result = new WorkflowExecutionResult("inst-1", "Completed", largeOutput);

        // Assert
        Assert.Equal(1000, result.Output?.Count);
    }

    [Fact]
    public void WorkflowExecutionResult_WithLongFaultMessage_ShouldPreserveComplete()
    {
        // Arrange
        var longMessage = new string('x', 5000);

        // Act
        var result = new WorkflowExecutionResult(null, "Faulted", null, true, longMessage);

        // Assert
        Assert.Equal(5000, result.FaultMessage?.Length);
    }

    #endregion
}
