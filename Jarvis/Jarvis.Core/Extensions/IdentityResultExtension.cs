using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Jarvis.Core.Extensions
{
    public static class IdentityResultExtension
    {
        public static string ToMessage(this IdentityResult source)
        {
            return string.Join(';', source.Errors.Select(x => x.Description).ToList());
        }
    }
}
