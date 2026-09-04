using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Jarvis.Authentication.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
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
}
