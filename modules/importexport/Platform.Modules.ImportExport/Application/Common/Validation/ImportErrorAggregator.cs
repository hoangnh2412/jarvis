using Platform.Modules.ImportExport.Application.Import.Models;

namespace Platform.Modules.ImportExport.Application.Common.Validation;

/// <summary>
/// Builds <see cref="ImportErrorSummary"/> from flat cell errors in a single pass.
/// Call per sheet (or with same-sheet errors only). New <see cref="ImportErrorCodes"/> values
/// group automatically via the composite key.
/// </summary>
public static class ImportErrorAggregator
{
    public readonly record struct Result(
        IReadOnlyList<ImportErrorSummary> Summaries,
        int FailedRowCount);

    public static Result Aggregate(IReadOnlyList<ImportRowError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
            return new Result([], 0);

        var groups = new Dictionary<GroupKey, GroupState>(errors.Count);
        var failedRows = new HashSet<int>();

        foreach (var error in errors)
        {
            failedRows.Add(error.RowNumber);

            var key = new GroupKey(
                error.ErrorCode,
                error.ColumnName,
                error.FieldName,
                error.Message);

            if (!groups.TryGetValue(key, out var state))
            {
                state = new GroupState(error);
                groups[key] = state;
            }

            state.AddRow(error.RowNumber);
        }

        var summaries = new List<ImportErrorSummary>(groups.Count);
        foreach (var state in groups.Values)
            summaries.Add(state.ToSummary());

        summaries.Sort(static (a, b) =>
        {
            var rowCmp = FirstRow(a).CompareTo(FirstRow(b));
            if (rowCmp != 0)
                return rowCmp;

            var colCmp = StringComparer.OrdinalIgnoreCase.Compare(a.ColumnName, b.ColumnName);
            if (colCmp != 0)
                return colCmp;

            return StringComparer.OrdinalIgnoreCase.Compare(a.ErrorCode, b.ErrorCode);
        });

        return new Result(summaries, failedRows.Count);
    }

    private static int FirstRow(ImportErrorSummary summary) =>
        summary.RowNumbers.Count == 0 ? int.MaxValue : summary.RowNumbers[0];

    private readonly record struct GroupKey(
        string? ErrorCode,
        string? ColumnName,
        string? FieldName,
        string Message);

    private sealed class GroupState(ImportRowError first)
    {
        private readonly ImportRowError _first = first;
        private readonly List<int> _rows = [];
        private readonly HashSet<int> _seen = [];

        public void AddRow(int rowNumber)
        {
            if (_seen.Add(rowNumber))
                _rows.Add(rowNumber);
        }

        public ImportErrorSummary ToSummary()
        {
            _rows.Sort();
            return new ImportErrorSummary(
                Message: _first.Message,
                RowNumbers: _rows,
                ColumnName: _first.ColumnName,
                FieldName: _first.FieldName,
                ErrorCode: _first.ErrorCode);
        }
    }
}
