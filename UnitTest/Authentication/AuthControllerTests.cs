using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Jarvis.Authentication;
using Jarvis.Authentication.ApiKey;
using Jarvis.Authentication.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UnitTest.Authentication.Helpers;

namespace UnitTest.Authentication;

/// <summary>
/// Test hồi quy cho 3 fix trên branch:
/// <list type="bullet">
/// <item>Issue 3 — composite forward theo <c>bearerScheme</c> (hết hard-code <c>"Bearer"</c>).</item>
/// <item>Issue 2 — <see cref="ApiKeyProviderOptions.RequireConfigKey"/> cô lập theo realm.</item>
/// <item>Issue 1 — bỏ bề mặt options speculative chưa dùng.</item>
/// </list>
/// </summary>
public class AuthControllerTests
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    // ---------- Issue 3: composite dùng bearerScheme ----------

    /// <summary>Hằng scheme JWT Bearer được khai báo tập trung, trùng giá trị ASP.NET.</summary>
    [Fact]
    public void AUTH_C_01_Bearer_scheme_constant_matches_aspnet_default()
    {
        Assert.Equal("Bearer", JarvisAuthenticationSchemes.Bearer);
    }

    /// <summary>
    /// JWT đăng ký dưới scheme tên tùy chỉnh + composite <c>bearerScheme</c> trỏ đúng tên đó —
    /// token hợp lệ được forward tới đúng handler và authenticate thành công.
    /// (Nếu còn hard-code <c>"Bearer"</c> thì forward tới scheme chưa đăng ký → 500.)
    /// </summary>
    [Fact]
    public async Task AUTH_C_02_Composite_forwards_bearer_to_custom_scheme()
    {
        await using var host = await CreateCustomBearerHostAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", CreateToken(SigningKey, DateTime.UtcNow.AddHours(1)));

        var response = await host.Client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        await AssertAuthenticatedAsync(response, expected: true);
    }

    /// <summary>
    /// Không có header nào khớp — composite fallback về <c>bearerScheme</c> tùy chỉnh (đã đăng ký),
    /// nên không ném lỗi mà chỉ trả về chưa authenticate.
    /// </summary>
    [Fact]
    public async Task AUTH_C_03_Composite_fallback_uses_custom_bearer_scheme()
    {
        await using var host = await CreateCustomBearerHostAsync();

        var response = await host.Client.GetAsync("/api/_auth-test/whoami");

        response.EnsureSuccessStatusCode();
        await AssertAuthenticatedAsync(response, expected: false);
    }

    // ---------- Issue 2: RequireConfigKey theo từng realm ----------

    /// <summary>
    /// Hai realm cấu hình <c>RequireConfigKey</c> khác nhau — validator dùng đúng cờ của
    /// <b>từng realm</b>, không bị ghi đè global (scheme sau không phá scheme trước).
    /// </summary>
    [Fact]
    public void AUTH_C_04_RequireConfigKey_isolated_per_realm()
    {
        var services = new ServiceCollection();
        services.Configure<ApiKeyProviderOptions>("Default", o => o.RequireConfigKey = true);
        services.Configure<ApiKeyProviderOptions>("Partner", o => o.RequireConfigKey = false);
        var monitor = services.BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<ApiKeyProviderOptions>>();
        var validator = new AuthenticationApiKeyOptionValidator(monitor);

        var defaultResult = validator.Validate(
            "Default", new AuthenticationApiKeyOption { KeyName = "X-API-KEY", Key = "" });
        var partnerResult = validator.Validate(
            "Partner", new AuthenticationApiKeyOption { KeyName = "X-API-KEY", Key = "" });

        Assert.True(defaultResult.Failed);
        Assert.Contains("Key is required", defaultResult.FailureMessage, StringComparison.OrdinalIgnoreCase);
        Assert.False(partnerResult.Failed);
    }

    /// <summary>
    /// <c>AddCoreApiKey&lt;ConfigApiKeyProvider&gt;</c> đăng ký named <see cref="ApiKeyProviderOptions"/>
    /// theo scheme với <c>RequireConfigKey=true</c> và <c>DefaultRealm</c> đúng.
    /// </summary>
    [Fact]
    public void AUTH_C_05_AddCoreApiKey_registers_named_require_config_key()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(key: "secret");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJarvisAuthentication(config, auth =>
            auth.AddCoreApiKey<ConfigApiKeyProvider>(config, JarvisAuthenticationSchemes.ApiKey));

        var monitor = services.BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<ApiKeyProviderOptions>>();
        var named = monitor.Get(JarvisAuthenticationSchemes.ApiKey);

        Assert.True(named.RequireConfigKey);
        Assert.Equal(JarvisAuthenticationSchemes.ApiKey, named.DefaultRealm);
    }

    // ---------- Issue 1: bề mặt options speculative đã bị xoá ----------

    /// <summary>Các option chưa từng được enforce đã bị gỡ khỏi public surface.</summary>
    [Fact]
    public void AUTH_C_06_Speculative_options_surface_removed()
    {
        Assert.Null(typeof(AuthenticationRootOptions).GetProperty("Cookie"));
        Assert.Null(typeof(AuthenticationRootOptions).GetProperty("PasswordExpiration"));
        Assert.Null(typeof(PasswordPolicyOptions).GetProperty("MaxFailedAttempts"));

        var configureJwt = typeof(Jarvis.Authentication.Jwt.AuthenticationBuilderExtension)
            .GetMethod(nameof(Jarvis.Authentication.Jwt.AuthenticationBuilderExtension.ConfigureJwtBearer));
        Assert.NotNull(configureJwt);
        Assert.Equal(2, configureJwt!.GetParameters().Length);
    }

    // ---------- Helpers ----------

    private static async Task<CustomBearerHost> CreateCustomBearerHostAsync()
    {
        const string jwtScheme = "InternalJwt";
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Type"] = "Jwt",
                ["Authentication:DefaultAuthenticateScheme"] = JarvisAuthenticationSchemes.Composite,
                ["Authentication:DefaultChallengeScheme"] = JarvisAuthenticationSchemes.Composite,
                [$"Authentication:Jwt:{jwtScheme}:IssuerSigningKeys:0"] = SigningKey,
                [$"Authentication:Jwt:{jwtScheme}:ValidateIssuerSigningKey"] = "true",
                [$"Authentication:Jwt:{jwtScheme}:ValidateAudience"] = "false",
                [$"Authentication:Jwt:{jwtScheme}:ValidateIssuer"] = "false",
                [$"Authentication:Jwt:{jwtScheme}:RequireHttpsMetadata"] = "false",
            })
            .Build();

        var host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddControllers()
                        .PartManager.ApplicationParts.Add(new AssemblyPart(typeof(TestAuthProbeController).Assembly));

                    services.AddJarvisAuthentication(config, auth =>
                    {
                        auth.AddJarvisCompositeScheme(bearerScheme: jwtScheme);
                        auth.AddCoreJwtBearer(config, jwtScheme);
                    });
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                });
            })
            .Build();

        await host.StartAsync();
        return new CustomBearerHost(host);
    }

    private static string CreateToken(string signingKey, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: [new Claim("sub", "user-1")],
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task AssertAuthenticatedAsync(HttpResponseMessage response, bool expected)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(expected, doc.RootElement.GetProperty("authenticated").GetBoolean());
    }

    private sealed class CustomBearerHost(IHost host) : IAsyncDisposable
    {
        public HttpClient Client { get; } = host.GetTestClient();

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await host.StopAsync();
            host.Dispose();
        }
    }
}
