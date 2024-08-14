using System.Security.Cryptography;
using System.Text;

namespace Jarvis.Shared.Extensions;

/// <summary>
/// Helper class provides functions for Hashing, Encrypt/Descrypt
/// </summary>
public static partial class CryptographyHelper
{
    /// <summary>
    /// Hash data from byte[]
    /// </summary>
    /// <param name="alg">Hash algorithm name. Ex: MD5, SHA1, SHA256, SHA384, SHA512</param>
    /// <param name="data">Data for hashing</param>
    /// <returns></returns>
    public static byte[] GetHash(HashAlgorithmName alg, byte[] data)
    {
        return HashAlgorithm.Create(alg.Name).ComputeHash(data);
    }

    /// <summary>
    /// Hash data from string
    /// </summary>
    /// <param name="alg">Hash algorithm name. Ex: MD5, SHA1, SHA256, SHA384, SHA512</param>
    /// <param name="data">Data for hashing</param>
    /// <returns></returns>
    public static byte[] GetHash(HashAlgorithmName alg, string data)
    {
        return GetHash(alg, Encoding.UTF8.GetBytes(data));
    }
}
