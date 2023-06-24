using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Core.Extensions
{
    public static class FormExtension
    {
        public static async Task<byte[]> ReadToBytesAsync(this IFormFile file)
        {
            if (file.Length == 0)
                return null;

            byte[] bytes = null;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                bytes = ms.ToArray();
            }
            return bytes;
        }
    }
}