using Platform.Modules.Files;
using Platform.Modules.Files.Models;
using Platform.Modules.Files.Validation;

namespace UnitTest.Files;

public class FilePathValidatorTests
{
    [Fact]
    public void ValidateUpload_Rejects_Traversal_In_FileName()
    {
        var result = FilePathValidator.ValidateUpload("invoices", "../secret.txt", 10);

        Assert.False(result.IsValid);
        Assert.Equal(FileErrorCode.InvalidPath, result.Code);
        Assert.Contains("fileName", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateUpload_Allows_Double_Dot_In_FileName()
    {
        var result = FilePathValidator.ValidateUpload("invoices", "report..final.pdf", 10);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateDownload_Rejects_Traversal_In_FileName()
    {
        var result = FilePathValidator.ValidateDownload("invoices", "folder/../secret.pdf");

        Assert.False(result.IsValid);
        Assert.Equal(FileErrorCode.InvalidPath, result.Code);
    }

    [Fact]
    public void ValidateView_Rejects_Empty_FileName()
    {
        var result = FilePathValidator.ValidateView("invoices", "  ");

        Assert.False(result.IsValid);
        Assert.Equal(FileErrorCode.RequiredField, result.Code);
    }

    [Fact]
    public void ValidateDelete_Rejects_Traversal_In_Prefix()
    {
        var result = FilePathValidator.ValidateDelete("invoices", fileName: null, prefix: "../other");

        Assert.False(result.IsValid);
        Assert.Equal(FileErrorCode.InvalidPath, result.Code);
        Assert.Contains("prefix", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateDelete_Rejects_Whitespace_Prefix_When_Provided()
    {
        var result = FilePathValidator.ValidateDelete("invoices", fileName: null, prefix: "   ");

        Assert.False(result.IsValid);
        Assert.Equal(FileErrorCode.WhitespaceField, result.Code);
    }

    [Fact]
    public void ValidateDelete_Rejects_Traversal_In_FileName()
    {
        var result = FilePathValidator.ValidateDelete("invoices", fileName: "..\\escape", prefix: null);

        Assert.False(result.IsValid);
        Assert.Equal(FileErrorCode.InvalidPath, result.Code);
    }

    [Fact]
    public void ValidateDeleteBatch_Rejects_Traversal_In_Paths()
    {
        var result = FilePathValidator.ValidateDeleteBatch("invoices", ["ok.pdf", "../bad.pdf"]);

        Assert.False(result.IsValid);
        Assert.Equal(FileErrorCode.InvalidPath, result.Code);
    }

    [Fact]
    public void ValidateMoveBatch_Rejects_Duplicate_Paths()
    {
        var result = FilePathValidator.ValidateMoveBatch(new MoveBatchFileInput
        {
            DestinationFolder = "archive",
            SourcePaths = ["2024/a.pdf", "2024/a.pdf"]
        });

        Assert.False(result.IsValid);
        Assert.Equal(FileErrorCode.DuplicatePath, result.Code);
    }

    [Fact]
    public void ValidateMove_Rejects_Destination_Inside_Source()
    {
        var result = FilePathValidator.ValidateMove(new MoveFileInput
        {
            SourcePath = "2024",
            DestinationFolder = "2024/Q1"
        });

        Assert.False(result.IsValid);
        Assert.Equal(FileErrorCode.InvalidMove, result.Code);
    }

    [Theory]
    [InlineData(FileErrorCode.InvalidPath)]
    [InlineData(FileErrorCode.TreeNotSupported)]
    [InlineData(FileErrorCode.EmptyFile)]
    public void FileErrorCode_Message_Returns_NonEmpty(string code)
    {
        var message = FileErrorCode.Message(code);

        Assert.False(string.IsNullOrWhiteSpace(message));
    }
}
