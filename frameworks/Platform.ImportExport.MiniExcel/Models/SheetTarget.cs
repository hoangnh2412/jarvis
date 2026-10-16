namespace Platform.ImportExport.MiniExcel.Models;

/// <summary>Resolved import target: template plan + actual workbook sheet name.</summary>
internal sealed record SheetTarget(SheetTemplate Plan, string TargetSheetName);
