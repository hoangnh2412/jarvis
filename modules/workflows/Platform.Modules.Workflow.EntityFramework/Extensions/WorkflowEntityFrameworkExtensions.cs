using Platform.Modules.ElsaWorkflow.Extensions;

namespace Platform.Modules.ElsaWorkflow.EntityFramework.Extensions;

public static class WorkflowEntityFrameworkExtensions
{
    /// <summary>
    /// Marker extension for explicit workflow persistence module composition (mặc định PostgreSQL).
    /// </summary>
    public static ElsaWorkflowModuleBuilder UseEntityFramework(this ElsaWorkflowModuleBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder;
    }

}
