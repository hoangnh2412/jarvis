using Jarvis.BlobStoring.Helpers;

namespace UnitTest.BlobStoring;

public class BlobPathHelperTests
{
    [Fact]
    public void Combine_Rejects_Traversal_In_FileName()
    {
        var root = Path.GetTempPath();
        Assert.Throws<InvalidOperationException>(() =>
            BlobPathHelper.Combine(root, "", "bucket", "../secret.txt"));
    }
}
