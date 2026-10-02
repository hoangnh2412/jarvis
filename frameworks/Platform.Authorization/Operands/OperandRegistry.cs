namespace Platform.Authorization.Operands;

using System.Diagnostics.CodeAnalysis;

/// <summary>Bộ đánh dấu DI cho một đóng góp khởi động cho danh sách operand.</summary>
public sealed class OperandContribution(Action<OperandCatalog> configure)
{
    internal Action<OperandCatalog> Configure { get; } = configure ?? throw new ArgumentNullException(nameof(configure));
}

public sealed class OperandRegistry : IOperandRegistry
{
    private readonly IReadOnlyDictionary<string, OperandDefinition> _items;

    private OperandRegistry(IEnumerable<OperandDefinition> items)
    {
        _items = items.ToDictionary(i => i.Path, StringComparer.Ordinal);
    }

    public IReadOnlyCollection<OperandDefinition> All => _items.Values.ToList();

    public bool TryGet(string path, [NotNullWhen(true)] out OperandDefinition? definition) =>
        _items.TryGetValue(path ?? string.Empty, out definition);

    /// <summary>Các mặc định của platform trước tiên, sau đó là các đóng góp theo thứ tự đăng ký.</summary>
    public static OperandRegistry Build(IEnumerable<OperandContribution> contributions)
    {
        var catalog = new OperandCatalog().AddPlatformDefaults();
        foreach (var contribution in contributions)
            contribution.Configure(catalog);
        return new OperandRegistry(catalog.Items);
    }

    public static OperandRegistry Build(params Action<OperandCatalog>[] configure) =>
        Build(configure.Select(c => new OperandContribution(c)));
}
