using Platform.ImportExport.MiniExcel.Validation;
using Platform.Modules.ImportExport.EntityFramework.Stores;

namespace UnitTest.ImportExport.Validation;

/// <summary>
/// Unit test cho <see cref="DefaultRowValidator"/> — rule Unique và MaxLength ở mức từng dòng.
/// </summary>
public class DefaultRowValidatorTests
{
    /// <summary>
    /// Cột Unique: dòng thứ hai trùng giá trị → lỗi DuplicateValue.
    /// Setup: mapping Email Unique=true; validate "a@b.com" hai lần với cùng SeenUniqueValues.
    /// Kỳ vọng: lần 1 không lỗi; lần 2 có ErrorCode DuplicateValue.
    /// </summary>
    [Fact]
    public async Task Unique_SecondRow_ReturnsDuplicateValue()
    {
        var sut = new DefaultRowValidator(new DefaultUniqueValueChecker());
        var mappings = new[] { new ColumnMapping("Email", "Email", Unique: true) };
        var seen = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        var first = await sut.ValidateAsync(
            new Dictionary<string, object?> { ["Email"] = "a@b.com" },
            new RowValidationContext
            {
                ExcelRowNumber = 2,
                Mappings = mappings,
                SeenUniqueValues = seen
            });

        var second = await sut.ValidateAsync(
            new Dictionary<string, object?> { ["Email"] = "a@b.com" },
            new RowValidationContext
            {
                ExcelRowNumber = 3,
                Mappings = mappings,
                SeenUniqueValues = seen
            });

        Assert.Empty(first);
        Assert.Contains(second, e => e.ErrorCode == ImportErrorCodes.DuplicateValue);
    }

    /// <summary>
    /// Cột MaxLength: vượt độ dài cho phép → lỗi MaxLengthExceeded.
    /// Setup: Name MaxLength = 3; giá trị "ABCD" (4 ký tự).
    /// Kỳ vọng: errors chứa ErrorCode MaxLengthExceeded.
    /// </summary>
    [Fact]
    public async Task MaxLength_Exceeded_ReturnsMaxLengthExceeded()
    {
        var sut = new DefaultRowValidator(new DefaultUniqueValueChecker());
        var mappings = new[] { new ColumnMapping("Name", "Name", MaxLength: 3) };

        var errors = await sut.ValidateAsync(
            new Dictionary<string, object?> { ["Name"] = "ABCD" },
            new RowValidationContext
            {
                ExcelRowNumber = 2,
                Mappings = mappings,
                SeenUniqueValues = []
            });

        Assert.Contains(errors, e => e.ErrorCode == ImportErrorCodes.MaxLengthExceeded);
    }

    [Fact]
    public async Task MinLength_NotMet_ReturnsMinLengthNotMet()
    {
        var sut = new DefaultRowValidator(new DefaultUniqueValueChecker());
        var mappings = new[] { new ColumnMapping("Code", "Code", MinLength: 3) };

        var errors = await sut.ValidateAsync(
            new Dictionary<string, object?> { ["Code"] = "AB" },
            new RowValidationContext
            {
                ExcelRowNumber = 2,
                Mappings = mappings,
                SeenUniqueValues = []
            });

        Assert.Contains(errors, e => e.ErrorCode == ImportErrorCodes.MinLengthNotMet);
    }
}
