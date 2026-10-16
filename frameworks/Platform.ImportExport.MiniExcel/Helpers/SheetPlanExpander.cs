using Platform.ImportExport.MiniExcel.Models;

namespace Platform.ImportExport.MiniExcel.Helpers;

/// <summary>
/// Chuyển <see cref="TemplateConfiguration"/> thành danh sách <see cref="SheetTarget"/> cụ thể
/// (plan mappings + tên sheet thật trong workbook). Chỉ gọi <c>GetSheetNames</c> tối đa một lần.
/// </summary>
/// <remarks>
/// Dùng ở đầu <c>MiniExcelImportProcessor.ImportCoreAsync</c> trước khi stream từng sheet.
/// </remarks>
internal static class SheetPlanExpander
{
    /// <summary>
    /// Expand legacy / multi-sheet config thành một target import cho mỗi sheet cần đọc.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>1 plan, <c>SheetName</c> null/rỗng → import <b>mọi</b> sheet, cùng mappings.</item>
    /// <item>N plan (hoặc 1 plan có tên) → mỗi plan phải có <c>SheetName</c>; match case-insensitive.</item>
    /// </list>
    /// Stream phải seekable để rewind sau <c>GetSheetNames</c>.
    /// </remarks>
    public static IReadOnlyList<SheetTarget> Expand(
        Stream stream,
        string fileName,
        TemplateConfiguration template)
    {
        // Normalize legacy Mappings/SheetName thành Sheets[] nếu cần
        var plans = template.ResolveSheetPlans();

        // Single plan without sheet name → all sheets in workbook (one GetSheetNames call)
        if (plans.Count == 1 && string.IsNullOrWhiteSpace(plans[0].SheetName))
        {
            var all = SheetNameResolver.GetSheetNames(stream, fileName);
            if (all.Count == 0)
                throw new InvalidTemplateException(EngineMessages.SheetNotFound("(any)", []));

            var targets = new List<SheetTarget>(all.Count);
            foreach (var name in all)
                targets.Add(new SheetTarget(plans[0], name));
            return targets;
        }

        // Named sheets: resolve against one sheet-name list (avoid N× GetSheetNames)
        var sheetNames = SheetNameResolver.GetSheetNames(stream, fileName);
        var result = new List<SheetTarget>(plans.Count);
        foreach (var plan in plans)
        {
            if (string.IsNullOrWhiteSpace(plan.SheetName))
                throw new InvalidTemplateException(EngineMessages.SheetNameRequiredForMultiSheet);

            var resolved = SheetNameResolver.Match(plan.SheetName, sheetNames)
                           ?? throw new InvalidTemplateException(
                               EngineMessages.SheetNotFound(plan.SheetName, sheetNames));
            result.Add(new SheetTarget(plan, resolved));
        }

        return result;
    }
}
