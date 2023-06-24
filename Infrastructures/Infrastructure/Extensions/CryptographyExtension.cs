using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Extensions
{
    public static class CryptographyExtension
    {
        public static byte[] GetHash(HashAlgorithmName alg, byte[] bytes)
        {
            return HashAlgorithm.Create(alg.Name).ComputeHash(bytes);
        }

        public static byte[] GetHash(HashAlgorithmName alg, string input)
        {
            return HashAlgorithm.Create(alg.Name).ComputeHash(Encoding.UTF8.GetBytes(input));
        }
    }
}