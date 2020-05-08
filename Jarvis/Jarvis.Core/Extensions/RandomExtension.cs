using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jarvis.Core.Extensions
{
    public static class RandomExtension
    {
        public static string Random(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string RandomUpperCase(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string RandomByGuid()
        {
            return Guid.NewGuid().GetHashCode().ToString("x");
        }

        public static string RandomByDateTime()
        {
            return DateTime.Now.Ticks.GetHashCode().ToString("x");
        }
    }
}
