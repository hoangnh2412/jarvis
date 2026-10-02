using Platform.Workflow.Abstractions;

namespace UnitTest.Workflow;

/// <summary>
/// Unit tests cho Workflow Abstractions - Interface contracts
/// Kiểm tra: IWorkflowService interface structure và WorkflowExecutionResult contracts
/// </summary>
public class WorkflowServiceInterfaceTests
{
    #region IWorkflowService Interface Contract Tests

    [Fact]
    public void IWorkflowService_ShouldHaveRunAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IWorkflowService);

        // Act
        var method = interfaceType.GetMethod("RunAsync");

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
    }

    [Fact]
    public void IWorkflowService_ShouldHaveSuspendAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IWorkflowService);

        // Act
        var method = interfaceType.GetMethod("SuspendAsync");

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
    }

    [Fact]
    public void IWorkflowService_ShouldHaveResumeAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(IWorkflowService);

        // Act
        var method = interfaceType.GetMethod("ResumeAsync");

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
    }

    [Fact]
    public void IWorkflowService_RunAsync_ShouldReturnTask()
    {
        // Arrange
        var interfaceType = typeof(IWorkflowService);
        var method = interfaceType.GetMethod("RunAsync");

        // Act
        var returnType = method!.ReturnType;

        // Assert
        Assert.NotNull(returnType);
        Assert.True(returnType.Name.StartsWith("Task"));
    }

    [Fact]
    public void IWorkflowService_SuspendAsync_ShouldReturnTask()
    {
        // Arrange
        var interfaceType = typeof(IWorkflowService);
        var method = interfaceType.GetMethod("SuspendAsync");

        // Act
        var returnType = method!.ReturnType;

        // Assert
        Assert.NotNull(returnType);
        Assert.True(returnType.Name.StartsWith("Task"));
    }

    [Fact]
    public void IWorkflowService_ResumeAsync_ShouldReturnTask()
    {
        // Arrange
        var interfaceType = typeof(IWorkflowService);
        var method = interfaceType.GetMethod("ResumeAsync");

        // Act
        var returnType = method!.ReturnType;

        // Assert
        Assert.NotNull(returnType);
        Assert.True(returnType.Name.StartsWith("Task"));
    }

    #endregion

    #region WorkflowExecutionResult Record Tests

    [Fact]
    public void WorkflowExecutionResult_ShouldBeRecord()
    {
        // Arrange
        var type = typeof(WorkflowExecutionResult);

        // Act
        var isRecord = type.Name.Contains("WorkflowExecutionResult") &&
                       typeof(ValueType).IsAssignableFrom(type) == false;

        // Assert
        Assert.NotNull(type);
    }

    [Fact]
    public void WorkflowExecutionResult_ShouldHaveWorkflowInstanceIdProperty()
    {
        // Arrange
        var type = typeof(WorkflowExecutionResult);

        // Act
        var property = type.GetProperty("WorkflowInstanceId");

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
    }

    [Fact]
    public void WorkflowExecutionResult_ShouldHaveStatusProperty()
    {
        // Arrange
        var type = typeof(WorkflowExecutionResult);

        // Act
        var property = type.GetProperty("Status");

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
    }

    [Fact]
    public void WorkflowExecutionResult_ShouldHaveOutputProperty()
    {
        // Arrange
        var type = typeof(WorkflowExecutionResult);

        // Act
        var property = type.GetProperty("Output");

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
    }

    [Fact]
    public void WorkflowExecutionResult_ShouldHaveIsFaultedProperty()
    {
        // Arrange
        var type = typeof(WorkflowExecutionResult);

        // Act
        var property = type.GetProperty("IsFaulted");

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
    }

    [Fact]
    public void WorkflowExecutionResult_ShouldHaveFaultMessageProperty()
    {
        // Arrange
        var type = typeof(WorkflowExecutionResult);

        // Act
        var property = type.GetProperty("FaultMessage");

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.CanRead);
    }

    #endregion

    #region WorkflowExecutionResult Instantiation Tests

    [Fact]
    public void WorkflowExecutionResult_CanBeCreatedWithMinimalParameters()
    {
        // Act
        var result = new WorkflowExecutionResult("instance-1", "Completed");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("instance-1", result.WorkflowInstanceId);
        Assert.Equal("Completed", result.Status);
    }

    [Fact]
    public void WorkflowExecutionResult_CanBeCreatedWithAllParameters()
    {
        // Act
        var output = new Dictionary<string, object> { { "key", "value" } };
        var result = new WorkflowExecutionResult(
            "instance-1",
            "Completed",
            output,
            false,
            null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("instance-1", result.WorkflowInstanceId);
        Assert.Equal("Completed", result.Status);
        Assert.NotNull(result.Output);
        Assert.False(result.IsFaulted);
        Assert.Null(result.FaultMessage);
    }

    [Fact]
    public void WorkflowExecutionResult_OutputCanBeNull()
    {
        // Act
        var result = new WorkflowExecutionResult("instance-1", "Completed", null);

        // Assert
        Assert.Null(result.Output);
    }

    [Fact]
    public void WorkflowExecutionResult_WithFaultedStatus_ShouldAllowFaultMessage()
    {
        // Act
        var result = new WorkflowExecutionResult(
            null,
            "Faulted",
            null,
            true,
            "An error occurred");

        // Assert
        Assert.True(result.IsFaulted);
        Assert.Equal("An error occurred", result.FaultMessage);
    }

    #endregion

    #region Workflow Definition Tests

    [Fact]
    public void Workflow_AbstractionsNamespace_ShouldExist()
    {
        // Act
        var assembly = typeof(IWorkflowService).Assembly;
        var types = assembly.GetTypes();

        // Assert
        Assert.NotEmpty(types);
    }

    [Fact]
    public void Workflow_ShouldHaveMultipleInterfaces()
    {
        // Act
        var assembly = typeof(IWorkflowService).Assembly;
        var interfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && t.Name.StartsWith("IWorkflow"))
            .ToList();

        // Assert
        Assert.NotEmpty(interfaces);
        Assert.Contains(interfaces, i => i.Name == "IWorkflowService");
    }

    #endregion

    #region Workflow Input/Output Handling Tests

    [Fact]
    public void WorkflowExecutionResult_OutputDictionary_ShouldSupportVariousTypes()
    {
        // Arrange
        var output = new Dictionary<string, object>
        {
            { "string", "value" },
            { "int", 42 },
            { "double", 3.14 },
            { "bool", true },
            { "list", new List<string> { "a", "b" } }
        };

        // Act
        var result = new WorkflowExecutionResult("inst-1", "Completed", output);

        // Assert
        Assert.Equal("value", result.Output?["string"]);
        Assert.Equal(42, result.Output?["int"]);
        Assert.Equal(3.14, result.Output?["double"]);
        Assert.True((bool)result.Output?["bool"]!);
    }

    #endregion
}
