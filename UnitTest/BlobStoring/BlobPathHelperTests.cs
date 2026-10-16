using Platform.BlobStoring.Helpers;

namespace UnitTest.BlobStoring;

public class BlobPathHelperTests
{
    [Fact]
    public void Combine_Allows_Empty_Bucket_At_Storage_Root()
    {
        var root = Path.Combine(Path.GetTempPath(), "platform-blob-path-" + Guid.NewGuid().ToString("N"));
        try
        {
            var path = BlobPathHelper.Combine(root, "", "", "root-file.txt");
            Assert.EndsWith("root-file.txt", path, StringComparison.OrdinalIgnoreCase);
            Assert.True(BlobPathHelper.IsUnderRoot(path, root));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Combine_Rejects_Traversal_In_FileName()
    {
        var root = Path.GetTempPath();
        Assert.Throws<InvalidOperationException>(() =>
            BlobPathHelper.Combine(root, "", "bucket", "../secret.txt"));
    }

    [Fact]
    public void Combine_Allows_Double_Dot_In_FileName()
    {
        var root = Path.Combine(Path.GetTempPath(), "platform-blob-path-" + Guid.NewGuid().ToString("N"));
        try
        {
            var path = BlobPathHelper.Combine(root, "", "bucket", "report..final.pdf");
            Assert.EndsWith("report..final.pdf", path, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void IsUnderRoot_Rejects_Prefix_Bypass()
    {
        Assert.False(BlobPathHelper.IsUnderRoot(@"C:\data\blobs\secret.txt", @"C:\data\blob"));
        Assert.True(BlobPathHelper.IsUnderRoot(@"C:\data\blob\invoices\a.pdf", @"C:\data\blob"));
    }

    [Theory]
    [InlineData("../secret", true)]
    [InlineData("folder/../secret", true)]
    [InlineData(@"s\..\blobs", true)]
    [InlineData("report..final.pdf", false)]
    [InlineData("invoices/2024/a.pdf", false)]
    public void ContainsTraversalSegment_Detects_Only_Path_Segments(string path, bool expected)
    {
        Assert.Equal(expected, BlobPathHelper.ContainsTraversalSegment(path));
    }
}
