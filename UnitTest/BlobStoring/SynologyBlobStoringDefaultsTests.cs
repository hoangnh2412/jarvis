using Platform.BlobStoring.Synology;

namespace UnitTest.BlobStoring;

public class SynologyBlobStoringDefaultsTests
{
    [Fact]
    public void ConfigurationSection_Is_Synology()
    {
        Assert.Equal("Synology", SynologyBlobStoringDefaults.ConfigurationSection);
    }

    [Fact]
    public void DefaultAutoSelectPriority_Is_23()
    {
        Assert.Equal(23, SynologyBlobStoringDefaults.DefaultAutoSelectPriority);
    }

    [Fact]
    public void ResolveAutoSelectPriority_Uses_Default_When_Zero_Or_Negative()
    {
        Assert.Equal(
            SynologyBlobStoringDefaults.DefaultAutoSelectPriority,
            SynologyBlobStoringDefaults.ResolveAutoSelectPriority(new SynologyBlobOptions()));

        Assert.Equal(
            SynologyBlobStoringDefaults.DefaultAutoSelectPriority,
            SynologyBlobStoringDefaults.ResolveAutoSelectPriority(
                new SynologyBlobOptions { AutoSelectPriority = -1 }));
    }

    [Fact]
    public void ResolveAutoSelectPriority_Uses_Configured_Positive_Value()
    {
        Assert.Equal(
            40,
            SynologyBlobStoringDefaults.ResolveAutoSelectPriority(
                new SynologyBlobOptions { AutoSelectPriority = 40 }));
    }

    [Theory]
    [InlineData(null, null, null, false)]
    [InlineData("", "user", "pass", false)]
    [InlineData("nas.local", "", "pass", false)]
    [InlineData("nas.local", "user", "", false)]
    [InlineData("nas.local", "user", "pass", true)]
    [InlineData("  nas.local  ", "  user  ", "pass", true)]
    public void IsEnabled_Requires_Host_Username_And_Password(
        string? host,
        string? username,
        string? password,
        bool expected)
    {
        var options = new SynologyBlobOptions
        {
            Host = host ?? string.Empty,
            Username = username ?? string.Empty,
            Password = password ?? string.Empty
        };

        Assert.Equal(expected, SynologyBlobStoringDefaults.IsEnabled(options));
    }

    [Theory]
    [InlineData(105, true)]
    [InlineData(106, true)]
    [InlineData(119, true)]
    [InlineData(401, true)]
    [InlineData(400, false)]
    public void IsSessionExpiredError_Detects_Session_Codes(int code, bool expected)
    {
        Assert.Equal(expected, SynologyBlobStoringDefaults.IsSessionExpiredError(code));
    }

    [Theory]
    [InlineData(0, false, 5000)]
    [InlineData(443, true, 443)]
    [InlineData(5001, false, 5001)]
    public void ResolvePort_Uses_Configured_Or_Default(int port, bool useHttps, int expected)
    {
        Assert.Equal(
            expected,
            SynologyBlobStoringDefaults.ResolvePort(new SynologyBlobOptions
            {
                Port = port,
                UseHttps = useHttps
            }));
    }
}
