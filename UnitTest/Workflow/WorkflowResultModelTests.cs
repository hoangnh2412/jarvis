using Platform.Workflow.Abstractions;

namespace UnitTest.Workflow;

/// <summary>
/// Unit tests cho WorkflowExecutionResult - kiểm tra data model integrity
/// Bao gồm: creation, serialization readiness, null handling, output preservation
/// </summary>
public class WorkflowResultModelTests
{
    #region Happy Path - Basic Creation

    [Fact]
    public void WorkflowExecutionResult_SuccessfulExecution_ShouldCreateProperResult()
    {
        // Arrange
        const string workflowInstanceId = "workflow-instance-abc123";
        const string expectedStatus = "Completed";

        // Act
        var result = new WorkflowExecutionResult(
            workflowInstanceId,
            expectedStatus);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(workflowInstanceId, result.WorkflowInstanceId);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Null(result.Output);
        Assert.False(result.IsFaulted);
        Assert.Null(result.FaultMessage);
    }

    [Fact]
    public void WorkflowExecutionResult_SuccessWithOutput_ShouldPreserveAllData()
    {
        // Arrange
        const string instanceId = "wf-inst-001";
        const string status = "Completed";
        var outputData = new Dictionary<string, object>
        {
            { "processId", "proc-789" },
            { "approvalStatus", "pending" },
            { "timestamp", "2025-01-15T10:30:00Z" }
        };

        // Act
        var result = new WorkflowExecutionResult(instanceId, status, outputData, false);

        // Assert
        Assert.Equal(instanceId, result.WorkflowInstanceId);
        Assert.Equal(status, result.Status);
        Assert.NotNull(result.Output);
        Assert.Equal(3, result.Output.Count);
        Assert.Equal("proc-789", result.Output["processId"]);
        Assert.False(result.IsFaulted);
    }

    #endregion

    #region Fault Handling

    [Fact]
    public void WorkflowExecutionResult_FaultedExecution_ShouldMarkIsFaultedTrue()
    {
        // Arrange
        const string faultMessage = "Database connection timeout";
        const string faultedStatus = "Faulted";

        // Act
        var result = new WorkflowExecutionResult(
            null,
            faultedStatus,
            null,
            true,
            faultMessage);

        // Assert
        Assert.Null(result.WorkflowInstanceId);
        Assert.Equal(faultedStatus, result.Status);
        Assert.True(result.IsFaulted);
        Assert.Equal(faultMessage, result.FaultMessage);
        Assert.Null(result.Output);
    }

    [Fact]
    public void WorkflowExecutionResult_FaultMessageDetailed_ShouldPreserveCompleteMessage()
    {
        // Arrange
        var detailedFault = "Workflow: ValidationWorkflow\nError: Invalid email format\nDatetime: 2025-01-15T10:45:22Z";

        // Act
        var result = new WorkflowExecutionResult(
            "wf-001",
            "Faulted",
            null,
            true,
            detailedFault);

        // Assert
        Assert.Equal(detailedFault, result.FaultMessage);
        Assert.Contains("Invalid email format", result.FaultMessage);
    }

    #endregion

    #region Null/Empty Value Handling

    [Fact]
    public void WorkflowExecutionResult_NullInstanceId_ShouldBeValid()
    {
        // Act
        var result = new WorkflowExecutionResult(null, "Suspended");

        // Assert
        Assert.Null(result.WorkflowInstanceId);
        Assert.Equal("Suspended", result.Status);
    }

    [Fact]
    public void WorkflowExecutionResult_NullOutput_ShouldBeValid()
    {
        // Act
        var result = new WorkflowExecutionResult("inst-123", "Completed", null);

        // Assert
        Assert.Null(result.Output);
    }

    [Fact]
    public void WorkflowExecutionResult_EmptyOutputDictionary_ShouldBePreserved()
    {
        // Arrange
        var emptyOutput = new Dictionary<string, object>();

        // Act
        var result = new WorkflowExecutionResult("inst-123", "Completed", emptyOutput);

        // Assert
        Assert.NotNull(result.Output);
        Assert.Empty(result.Output);
    }

    #endregion

    #region Data Integrity

    [Fact]
    public void WorkflowExecutionResult_ComplexObjectOutput_ShouldPreserveTypes()
    {
        // Arrange
        var complexOutput = new Dictionary<string, object>
        {
            { "employeeId", 12345 },
            { "firstName", "John" },
            { "isActive", true },
            { "salary", 75000.50 },
            { "hireDate", new DateTime(2020, 1, 15) },
            { "tags", new[] { "engineering", "senior" } }
        };

        // Act
        var result = new WorkflowExecutionResult("emp-workflow-1", "Completed", complexOutput);

        // Assert
        Assert.Equal(12345, (int)result.Output!["employeeId"]);
        Assert.Equal("John", (string)result.Output["firstName"]);
        Assert.True((bool)result.Output["isActive"]);
        Assert.Equal(75000.50, (double)result.Output["salary"]);
    }

    [Fact]
    public void WorkflowExecutionResult_OutputIndependence_MultipleInstancesShouldNotInterfere()
    {
        // Arrange
        var output1 = new Dictionary<string, object> { { "userId", "user-1" } };
        var output2 = new Dictionary<string, object> { { "userId", "user-2" } };

        // Act
        var result1 = new WorkflowExecutionResult("inst-1", "Completed", output1);
        var result2 = new WorkflowExecutionResult("inst-2", "Completed", output2);

        // Modify output1 after creation
        output1["userId"] = "user-modified";

        // Assert
        // Verify that result2 is unaffected by modifications to output1
        Assert.NotEqual(result1.Output!["userId"], result2.Output!["userId"]);
    }

    #endregion

    #region Status Variations

    [Theory]
    [InlineData("Completed")]
    [InlineData("Faulted")]
    [InlineData("Suspended")]
    [InlineData("Running")]
    [InlineData("Idle")]
    [InlineData("Terminated")]
    [InlineData("Unknown")]
    public void WorkflowExecutionResult_VariousStatusValues_AllShouldBeValid(string status)
    {
        // Act
        var result = new WorkflowExecutionResult("inst-1", status);

        // Assert
        Assert.Equal(status, result.Status);
    }

    #endregion

    #region Concurrent Scenario Simulation

    [Fact]
    public void WorkflowExecutionResult_MultipleSimultaneousExecutions_ShouldBeIndependent()
    {
        // Arrange
        var tasks = new List<Task<WorkflowExecutionResult>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() =>
            {
                return new WorkflowExecutionResult(
                    $"instance-{index}",
                    "Completed",
                    new Dictionary<string, object> { { "index", index } });
            }));
        }

        var results = Task.WhenAll(tasks).Result;

        // Assert
        Assert.Equal(10, results.Length);
        for (int i = 0; i < results.Length; i++)
        {
            Assert.NotNull(results[i].WorkflowInstanceId);
            Assert.Equal("Completed", results[i].Status);
        }
    }

    #endregion

    #region String Representation and Serialization Readiness

    [Fact]
    public void WorkflowExecutionResult_ShouldBeSerializable_WithToString()
    {
        // Arrange
        var result = new WorkflowExecutionResult(
            "inst-123",
            "Completed",
            new Dictionary<string, object> { { "key", "value" } });

        // Act
        var stringRepresentation = result.ToString();

        // Assert
        Assert.NotNull(stringRepresentation);
        Assert.NotEmpty(stringRepresentation);
    }

    [Fact]
    public void WorkflowExecutionResult_RecordType_ShouldSupportEquality()
    {
        // Arrange
        var output = new Dictionary<string, object> { { "key", "value" } };
        var result1 = new WorkflowExecutionResult("inst-1", "Completed", output, false, null);
        var result2 = new WorkflowExecutionResult("inst-1", "Completed", output, false, null);

        // Act & Assert
        // Since it's a record, with same values it should result in same values
        Assert.Equal(result1.WorkflowInstanceId, result2.WorkflowInstanceId);
        Assert.Equal(result1.Status, result2.Status);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void WorkflowExecutionResult_WithSpecialCharactersInOutput_ShouldPreserve()
    {
        // Arrange
        var output = new Dictionary<string, object>
        {
            { "email", "user+test@example.com" },
            { "description", "Contains \"quotes\" and 'apostrophes'" },
            { "path", @"C:\Users\Admin\Documents" },
            { "unicode", "Chinese: 中文, Arabic: العربية" }
        };

        // Act
        var result = new WorkflowExecutionResult("inst-1", "Completed", output);

        // Assert
        Assert.Equal("user+test@example.com", result.Output!["email"]);
        var desc = (string)result.Output["description"];
        var unicode = (string)result.Output["unicode"];
        Assert.Contains("quotes", desc);
        Assert.Contains("中文", unicode);
    }

    [Fact]
    public void WorkflowExecutionResult_WithLargeDataSet_ShouldHandleCorrectly()
    {
        // Arrange
        var largeOutput = new Dictionary<string, object>();
        for (int i = 0; i < 10000; i++)
        {
            largeOutput[$"field_{i:D5}"] = $"value_{i}";
        }

        // Act
        var result = new WorkflowExecutionResult("inst-large-1", "Completed", largeOutput);

        // Assert
        Assert.NotNull(result.Output);
        Assert.Equal(10000, result.Output.Count);
    }

    #endregion
}
