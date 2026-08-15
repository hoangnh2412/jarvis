using Microsoft.Extensions.Configuration;

namespace UnitTest.Authentication.Helpers;

/// <summary>Builder tạo <see cref="IConfiguration"/> in-memory cho test authentication.</summary>
internal static class AuthenticationConfigurationBuilder
{
    public static IConfiguration BuildApiKeyConfig(string realm = "Default", string key = "secret") =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Type"] = "ApiKey",
                ["Authentication:ApiKey:" + realm + ":KeyName"] = "X-API-KEY",
                ["Authentication:ApiKey:" + realm + ":Key"] = key,
            })
            .Build();

    public static IConfiguration BuildBasicConfig(
        string username = "testuser",
        string password = "testpass",
        string realm = "Test Realm",
        params string[] roles)
    {
        var data = new Dictionary<string, string?>
        {
            ["Authentication:Type"] = "Basic",
            ["Authentication:DefaultAuthenticateScheme"] = "Basic",
            ["Authentication:DefaultChallengeScheme"] = "Basic",
            ["Authentication:Basic:Default:Realm"] = realm,
            ["Authentication:Basic:Default:Users:" + username + ":Password"] = password,
        };

        for (var i = 0; i < roles.Length; i++)
            data["Authentication:Basic:Default:Users:" + username + ":Roles:" + i] = roles[i];

        return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
    }

    public static IConfiguration BuildJwtSymmetricConfig(
        string signingKey = "test-signing-key-at-least-32-chars-long",
        bool requireHttpsMetadata = false) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Type"] = "Jwt",
                ["Authentication:Jwt:Bearer:IssuerSigningKeys:0"] = signingKey,
                ["Authentication:Jwt:Bearer:ValidateIssuerSigningKey"] = "true",
                ["Authentication:Jwt:Bearer:ValidateAudience"] = "false",
                ["Authentication:Jwt:Bearer:ValidateIssuer"] = "false",
                ["Authentication:Jwt:Bearer:RequireHttpsMetadata"] = requireHttpsMetadata.ToString(),
            })
            .Build();

    public static IConfiguration BuildRootConfig(Dictionary<string, string?> values)
    {
        var data = new Dictionary<string, string?>(values)
        {
            ["Authentication:Type"] = values.GetValueOrDefault("Authentication:Type") ?? "ApiKey"
        };
        return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
    }
}
