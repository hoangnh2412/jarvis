// Isolated namespace wrapper: calls third-party AddHealthChecksUI + AddInMemoryStorage to avoid extension name collisions.
using Jarvis.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace JarvisHealthChecksThirdPartyUi;

/// <summary>
/// Registers AspNetCore.HealthChecks.UI services outside the <c>Jarvis.HealthChecks</c> namespace to avoid type-name clashes.
/// Đăng ký dịch vụ UI ngoài namespace <c>Jarvis.HealthChecks</c> để tránh xung đột tên kiểu.
/// </summary>
/// <remarks>
/// Always calls <c>AddInMemoryStorage</c> from UI.InMemory.Storage (EF Core in-memory provider).
/// Luôn gọi <c>AddInMemoryStorage</c> từ UI.InMemory.Storage (provider EF Core in-memory).
/// </remarks>
internal static class HealthChecksUiThirdPartyRegistration
{
    /// <summary>
    /// EN: Adds UI configuration, endpoints, webhooks, and InMemory persistence / VI: Thêm cấu hình UI, endpoint, webhook và lưu InMemory.
    /// </summary>
    /// <param name="services">EN: Application service collection / VI: Bộ dịch vụ của ứng dụng.</param>
    /// <param name="snapshot">EN: Bound health options including UI subsection / VI: Options health đã bind gồm mục UI.</param>
    internal static void RegisterHealthChecksUi(IServiceCollection services, JarvisHealthCheckOptions snapshot)
    {
        var ui = snapshot.Ui;

        var uiBuilder = services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(Math.Clamp(ui.EvaluationTimeSeconds, 5, 300));
            foreach (var ep in ui.Endpoints.Where(e => !string.IsNullOrWhiteSpace(e.Uri)))
                setup.AddHealthCheckEndpoint(ep.Name, ep.Uri);
            foreach (var w in ui.Webhooks.Where(w => !string.IsNullOrWhiteSpace(w.Uri)))
                setup.AddWebhookNotification(w.Name, w.Uri, w.Payload, w.RestoredPayload);
        });

        uiBuilder.AddInMemoryStorage();
    }
}
