namespace Platform.ImportExport.MiniExcel.Helpers;

/// <summary>
/// Simple mutable counter for export/import row limits while streaming
/// (avoids closing over a local <c>int</c> inside iterators).
/// </summary>
internal sealed class RowCounter
{
    /// <summary>Number of rows accepted so far.</summary>
    public int Count { get; private set; }

    /// <summary>Increments the counter by one.</summary>
    public void Increment() => Count++;
}
