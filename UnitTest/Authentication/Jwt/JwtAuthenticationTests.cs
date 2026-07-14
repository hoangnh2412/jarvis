using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Jarvis.Authentication;
using Jarvis.Authentication.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using UnitTest.Authentication.Helpers;

namespace UnitTest.Authentication.Jwt;

/// <summary>Test JWT Bearer — symmetric key và token validation.</summary>
public class JwtAuthenticationTests
{
    /// <summary>Có <c>Authority</c> — không set symmetric <c>IssuerSigningKeys</c> (dùng metadata OIDC).</summary>
    [Fact]
    public void JWT_U_01_Authority_mode_does_not_set_symmetric_keys()
    {
        var options = new JwtBearerOptions();
        var jwtOption = new AuthenticationJwtOption
        {
            Authority = "https://localhost:5001/",
            Audience = "api",
            ValidateIssuerSigningKey = true
        };

        AuthenticationBuilderExtension.ConfigureJwtBearer(options, jwtOption, new ServiceCollection());

        Assert.Equal("https://localhost:5001", options.Authority);
        Assert.False(options.TokenValidationParameters.ValidateIssuerSigningKey);
        Assert.Null(options.TokenValidationParameters.IssuerSigningKeys);
    }

    /// <summary>Không Authority và không có signing key — validator startup fail.</summary>
    [Fact]
    public void JWT_U_03_Missing_keys_fails_validation()
    {
        var validator = new AuthenticationJwtOptionValidator();
        var result = validator.Validate("Bearer", new AuthenticationJwtOption
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = []
        });

        Assert.True(result.Failed);
    }

    /// <summary>Mặc định <c>RequireHttpsMetadata</c> = true khi không cấu hình.</summary>
    [Fact]
    public void JWT_U_04_Require_https_metadata_defaults_true()
    {
        var options = new JwtBearerOptions();
        AuthenticationBuilderExtension.ConfigureJwtBearer(options, new AuthenticationJwtOption(), new ServiceCollection());

        Assert.True(options.RequireHttpsMetadata);
    }

    /// <summary>Config <c>RequireHttpsMetadata: false</c> — override được.</summary>
    [Fact]
    public void JWT_U_05_Require_https_metadata_can_be_false()
    {
        var options = new JwtBearerOptions();
        AuthenticationBuilderExtension.ConfigureJwtBearer(options, new AuthenticationJwtOption
        {
            RequireHttpsMetadata = false
        }, new ServiceCollection());

        Assert.False(options.RequireHttpsMetadata);
    }

    /// <summary><c>AddCoreJwtBearer</c> đăng ký <see cref="AllowAllJwtTokenAccessChecker"/>.</summary>
    [Fact]
    public void JWT_U_06_Registers_allow_all_access_checker_by_default()
    {
        var config = AuthenticationConfigurationBuilder.BuildJwtSymmetricConfig("test-signing-key-at-least-32-chars-long");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJarvisAuthentication(config, auth => auth.AddCoreJwtBearer(config));
        var sp = services.BuildServiceProvider();

        Assert.IsType<AllowAllJwtTokenAccessChecker>(sp.GetRequiredService<IJwtTokenAccessChecker>());
    }

    /// <summary>Bearer token hợp lệ (symmetric) — <c>authenticated: true</c> trên endpoint probe.</summary>
    [Fact]
    public async Task JWT_I_01_Valid_bearer_token_authenticates()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var config = AuthenticationConfigurationBuilder.BuildJwtSymmetricConfig(signingKey);
        await using var server = await AuthenticationTestServer.CreateAsync(config, composite: false, jwt: true, apiKey: false);

        var token = CreateToken(signingKey, DateTime.UtcNow.AddHours(1));
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await server.Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"authenticated\":true", json, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Token không hợp lệ — không authenticate.</summary>
    [Fact]
    public async Task JWT_I_02_Invalid_bearer_not_authenticated()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var config = AuthenticationConfigurationBuilder.BuildJwtSymmetricConfig(signingKey);
        await using var server = await AuthenticationTestServer.CreateAsync(config, composite: false, jwt: true, apiKey: false);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "not.a.valid.jwt");

        var response = await server.Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"authenticated\":false", json, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary><see cref="IJwtTokenAccessChecker"/> trả <c>false</c> — token chữ ký đúng vẫn không authenticate.</summary>
    [Fact]
    public async Task JWT_I_03_Access_checker_can_reject_valid_token()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var config = AuthenticationConfigurationBuilder.BuildJwtSymmetricConfig(signingKey);

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
                        auth.AddCoreJwtBearer<RejectAllJwtTokenAccessChecker>(
                            config, JwtBearerDefaults.AuthenticationScheme));
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
        try
        {
            var client = host.GetTestClient();
            var token = CreateToken(signingKey, DateTime.UtcNow.AddHours(1), jti: "revoked-1");
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            Assert.False(doc.RootElement.GetProperty("authenticated").GetBoolean());
        }
        finally
        {
            await host.StopAsync();
            host.Dispose();
        }
    }

    private static string CreateToken(string signingKey, DateTime expires, string? jti = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim> { new("sub", "user-1") };
        if (jti is not null)
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, jti));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class RejectAllJwtTokenAccessChecker : IJwtTokenAccessChecker
    {
        public Task<bool> IsAllowedAsync(
            ClaimsPrincipal principal,
            string? rawToken,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}
