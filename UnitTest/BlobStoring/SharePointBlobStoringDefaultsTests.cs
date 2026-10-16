using Platform.BlobStoring.SharePoint;

namespace UnitTest.BlobStoring;

public class SharePointBlobStoringDefaultsTests
{
    [Fact]
    public void ResolveAutoSelectPriority_Uses_Default_When_Zero_Or_Negative()
    {
        Assert.Equal(
            SharePointBlobStoringDefaults.DefaultAutoSelectPriority,
            SharePointBlobStoringDefaults.ResolveAutoSelectPriority(new SharePointBlobOptions()));

        Assert.Equal(
            SharePointBlobStoringDefaults.DefaultAutoSelectPriority,
            SharePointBlobStoringDefaults.ResolveAutoSelectPriority(
                new SharePointBlobOptions { AutoSelectPriority = -1 }));
    }

    [Fact]
    public void ResolveAutoSelectPriority_Uses_Configured_Positive_Value()
    {
        Assert.Equal(
            40,
            SharePointBlobStoringDefaults.ResolveAutoSelectPriority(
                new SharePointBlobOptions { AutoSelectPriority = 40 }));
    }

    [Fact]
    public void ConfigurationSection_Is_SharePoint()
    {
        Assert.Equal("SharePoint", SharePointBlobStoringDefaults.ConfigurationSection);
    }

    [Fact]
    public void DefaultAutoSelectPriority_Is_25()
    {
        Assert.Equal(25, SharePointBlobStoringDefaults.DefaultAutoSelectPriority);
    }
}
