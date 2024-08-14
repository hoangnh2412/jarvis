namespace Jarvis.Domain.Shared.Utility;

/// <summary>
/// Provides helper functions for HEX
/// </summary>
public static partial class HexHelper
{
    public static byte[] HexToByteArray(string hex)
    {
        return Enumerable
            .Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }

    public static string ByteArrayToHex(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }

    public static bool IsHex(string data)
    {
        return int.TryParse(data, System.Globalization.NumberStyles.HexNumber, null, out _);
    }
}