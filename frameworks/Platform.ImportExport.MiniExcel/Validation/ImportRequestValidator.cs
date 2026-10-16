using Platform.ImportExport.MiniExcel.Helpers;
using Microsoft.Extensions.Options;

namespace Platform.ImportExport.MiniExcel.Validation;

/// <summary>
/// Validate <see cref="ImportWork"/> trước khi MiniExcel đọc file — fail-fast, không I/O sheet data.
/// </summary>
/// <remarks>
/// Kiểm tra: extension, kích thước stream, sheet plans, HeaderRow/StartRow/RowCount, mappings.
/// Consumer có thể thay bằng custom <see cref="IImportRequestValidator"/> (TryAdd không ghi đè).
/// </remarks>
public sealed class ImportRequestValidator(IOptions<ExcelEngineOptions> options) : IImportRequestValidator
{
    private readonly ExcelEngineOptions _options = options.Value;

    /// <inheritdoc />
    public void Validate(ImportWork work)
    {
        ArgumentNullException.ThrowIfNull(work);
        ArgumentNullException.ThrowIfNull(work.FileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(work.FileName);
        ArgumentNullException.ThrowIfNull(work.Template);

        // Chỉ .xlsx / .csv (hoặc list trong ExcelEngine:SupportedExtensions)
        if (!FileNameHelper.IsSupportedImportFile(work.FileName, _options.SupportedExtensions))
            throw new UnsupportedFileException(work.FileName);

        // Length chỉ tin cậy khi stream seekable (HTTP upload thường seekable sau buffer)
        if (work.FileStream.CanSeek && work.FileStream.Length > _options.MaxFileSizeBytes)
            throw new FileTooLargeException(work.FileStream.Length, _options.MaxFileSizeBytes);

        var plans = work.Template.ResolveSheetPlans();
        if (plans.Count == 0)
            throw new InvalidTemplateException(EngineMessages.MappingsRequired);

        // Multi-sheet: mỗi plan bắt buộc SheetName; single plan có thể null (= all sheets lúc expand)
        if (plans.Count > 1)
        {
            foreach (var plan in plans)
            {
                if (string.IsNullOrWhiteSpace(plan.SheetName))
                    throw new InvalidTemplateException(EngineMessages.SheetNameRequiredForMultiSheet);
                ValidateSheetPlan(plan);
            }
        }
        else
        {
            ValidateSheetPlan(plans[0]);
        }

        TemplateFileTypeHelper.ValidateSheetNameForTemplate(
            work.TemplateFileName,
            work.Template.SheetName,
            work.Template.Sheets);
    }

    /// <summary>
    /// Validate bounds một sheet plan: StartRow/HeaderRow quan hệ, RowCount &gt; 0, mappings hợp lệ.
    /// </summary>
    private static void ValidateSheetPlan(SheetTemplate plan)
    {
        if (plan.StartRow < 1)
            throw new InvalidTemplateException(EngineMessages.StartRowInvalid());

        if (plan.HeaderRow is < 1)
            throw new InvalidTemplateException(EngineMessages.HeaderRowInvalid());

        var headerRow = plan.ResolveHeaderRow();
        if (headerRow >= plan.StartRow)
            throw new InvalidTemplateException(EngineMessages.HeaderRowMustPrecedeStartRow());

        if (plan.RowCount is <= 0)
            throw new InvalidTemplateException(EngineMessages.RowCountInvalid());

        MappingValidator.Validate(plan.Mappings);
    }

    /// <summary>
    /// Header row hiệu dụng ở root template (legacy): explicit HeaderRow hoặc StartRow - 1.
    /// </summary>
    public static int ResolveHeaderRow(TemplateConfiguration template) =>
        template.HeaderRow ?? Math.Max(1, template.StartRow - 1);
}
