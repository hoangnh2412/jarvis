using System;
using System.Linq;

namespace Infrastructure.Extensions
{
    public static class FormatExtension
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
    }
}