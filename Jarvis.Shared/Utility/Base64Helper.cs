namespace Jarvis.Shared.Extensions;

/// <summary>
/// Provides helper functions for BASE64
/// </summary>
public static partial class Base64Helper
{
    public static byte[] Base64ToByteArray(string data)
    {
        return Convert.FromBase64String(data);
    }

    public static string ByteArrayToBase64(byte[] data)
    {
        return Convert.ToBase64String(data);
    }

    public static bool IsBase64(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return false;
        }

        try
        {
            var byteArray = Convert.FromBase64String(data);

            return byteArray.Any();
        }
        catch
        {
            return false;
        }
    }
}