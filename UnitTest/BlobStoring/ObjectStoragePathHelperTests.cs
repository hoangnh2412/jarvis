using Platform.BlobStoring.Helpers;

namespace UnitTest.BlobStoring;

public class ObjectStoragePathHelperTests
{
    [Fact]
    public void BuildMoveDestinationPath_Keeps_Source_Leaf_Name()
    {
        var result = ObjectStoragePathHelper.BuildMoveDestinationPath("2024/report.pdf", "archive");

        Assert.Equal("archive/report.pdf", result);
    }

    [Fact]
    public void IsDestinationInsideSource_Detects_Self_And_Subfolder()
    {
        Assert.True(ObjectStoragePathHelper.IsDestinationInsideSource("2024", "2024"));
        Assert.True(ObjectStoragePathHelper.IsDestinationInsideSource("2024", "2024/Q1"));
        Assert.False(ObjectStoragePathHelper.IsDestinationInsideSource("2024/Q1", "2024"));
        Assert.False(ObjectStoragePathHelper.IsDestinationInsideSource("2024/report.pdf", "archive"));
    }

    [Fact]
    public void OrderForBatchMove_Orders_Deepest_Paths_First()
    {
        var ordered = ObjectStoragePathHelper.OrderForBatchMove(
            ["test4", "test4/test5/a.txt", "test3"]);

        Assert.Equal(["test4/test5/a.txt", "test4", "test3"], ordered);
    }

    [Fact]
    public void ReplacePrefix_Replaces_Folder_Prefix()
    {
        var result = ObjectStoragePathHelper.ReplacePrefix(
            "invoices/2024/a.pdf",
            "invoices/2024",
            "invoices/2025");

        Assert.Equal("invoices/2025/a.pdf", result);
    }

    [Fact]
    public void CollectKeysUnderPrefix_Includes_Subtree_Keys()
    {
        var keys = new[] { "invoices/2024/a.pdf", "invoices/2024/b.pdf", "other.txt" };

        var affected = ObjectStoragePathHelper.CollectKeysUnderPrefix(keys, "invoices/2024").ToList();

        Assert.Equal(2, affected.Count);
        Assert.Contains("invoices/2024/a.pdf", affected);
        Assert.Contains("invoices/2024/b.pdf", affected);
    }
}
