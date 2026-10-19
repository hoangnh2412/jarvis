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
                ["Authentication:Schemes:ApiKey:Profiles:" + realm + ":Source"] = "Configuration",
                ["Authentication:Schemes:ApiKey:Profiles:" + realm + ":KeyName"] = "X-API-KEY",
                ["Authentication:Schemes:ApiKey:Profiles:" + realm + ":Key"] = key,
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
            ["Authentication:Schemes:Basic:Profiles:Default:Source"] = "Configuration",
            ["Authentication:Schemes:Basic:Profiles:Default:Realm"] = realm,
            ["Authentication:Schemes:Basic:Profiles:Default:Users:" + username + ":Password"] = password,
        };

        for (var i = 0; i < roles.Length; i++)
            data["Authentication:Schemes:Basic:Profiles:Default:Users:" + username + ":Roles:" + i] = roles[i];

        return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
    }

    public static IConfiguration BuildJwtSymmetricConfig(
        string signingKey = "test-signing-key-at-least-32-chars-long",
        bool requireHttpsMetadata = false) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Type"] = "Jwt",
                ["Authentication:Schemes:Bearer:Profiles:Default:IssuerSigningKeys:0"] = signingKey,
                ["Authentication:Schemes:Bearer:Profiles:Default:ValidateIssuerSigningKey"] = "true",
                ["Authentication:Schemes:Bearer:Profiles:Default:ValidateAudience"] = "false",
                ["Authentication:Schemes:Bearer:Profiles:Default:ValidateIssuer"] = "false",
                ["Authentication:Schemes:Bearer:Profiles:Default:RequireHttpsMetadata"] = requireHttpsMetadata.ToString(),
            })
            .Build();

    public static IConfiguration BuildRootConfig(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
